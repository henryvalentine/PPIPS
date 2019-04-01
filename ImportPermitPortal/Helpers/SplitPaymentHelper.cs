using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using ImportPermitPortal.DataObjects;
using ImportPermitPortal.DataObjects.Helpers;

namespace ImportPermitPortal.Helpers
{
    public class SplitPaymentHelper
    {
        public Lineitem[] ComputeApplicationPaymentSplit(double amount, List<FeeObject> fees)
        {
            try
            {
                if (amount < 1 || !fees.Any())
                {
                    return new Lineitem[] { };
                }

                const int statutory = (int)FeeTypeEnum.Statutory_Fee;
                const int processing = (int)FeeTypeEnum.Processing_Fee;

                var statutoryFee = fees.Find(f => f.FeeTypeId == statutory);
                var processingFee = fees.Find(f => f.FeeTypeId == processing);

                if (statutoryFee == null || statutoryFee.Amount < 1 || processingFee == null || processingFee.Amount < 1)
                {
                    return new Lineitem[] { };
                }

                var list = new List<Lineitem>();

                var factor = statutoryFee.Amount;
                var multiplier = (amount - processingFee.Amount) / factor;

                var fgSplit = statutoryFee.Amount * multiplier;
                var gateWayAddedSplit = (fgSplit)/100;
                var fgAmount = fgSplit - gateWayAddedSplit;
                var fgDetails = GetFgDetails(fgAmount.ToString(CultureInfo.InvariantCulture));
                
                list.Add(fgDetails);
                var principalSplit = processingFee.PrincipalSplit;
                var vendorSplit = 0.0;
                var gatewaySplit = 0.0;

                if (processingFee.VendorSplit > 0)
                {
                    vendorSplit = processingFee.VendorSplit;
                }

                if (processingFee.PaymentGatewaySplit > 0)
                {
                    gatewaySplit = processingFee.PaymentGatewaySplit;
                }

                if (gatewaySplit > 0)
                {
                    principalSplit = principalSplit + gatewaySplit + gateWayAddedSplit;
                
                }

                var principalDetails = GetDprDetails(principalSplit.ToString(CultureInfo.InvariantCulture));
                list.Add(principalDetails);

                var vendoDetails = GetMaxfrontDetails(vendorSplit.ToString(CultureInfo.InvariantCulture));
                list.Add(vendoDetails);

                return list.ToArray();

            }
            catch (Exception ex)
            {
                ErrorLogger.LoggError(ex.StackTrace, ex.Source, ex.Message);
                return new Lineitem[] { };
            }
        }

        public Lineitem[] ComputeNotificationPaymentSplit(List<FeeObject> fees)
        {
            try
            {
                if (!fees.Any())
                {
                    return new Lineitem[] { };
                }

                const int expenditionary = (int)FeeTypeEnum.Expeditionary;
                const int processing = (int)FeeTypeEnum.Processing_Fee;

                var expenditionaryFee = fees.Find(f => f.FeeTypeId == expenditionary);
                var processingFee = fees.Find(f => f.FeeTypeId == processing);

                if (processingFee == null || processingFee.Amount < 1)
                {
                    return new Lineitem[] { };
                }

                var list = new List<Lineitem>();

                var principalExpSplit = 0.0;
                var vendorExpSplit = 0.0;
                var principalProcSplit = 0.0;
                var vendorProcSplit = 0.0;

                if (expenditionaryFee != null && expenditionaryFee.Amount > 0)
                {
                    if (expenditionaryFee.VendorSplit > 0)
                    {
                        vendorExpSplit = expenditionaryFee.VendorSplit;
                    }

                    principalExpSplit = expenditionaryFee.PrincipalSplit;

                    if (expenditionaryFee.PaymentGatewaySplit > 0)
                    {
                        principalExpSplit = principalExpSplit + expenditionaryFee.PaymentGatewaySplit;
                        
                    }

                }
                else
                {
                    if (processingFee.VendorSplit > 0)
                    {
                        vendorProcSplit = processingFee.VendorSplit;
                    }

                    principalProcSplit = processingFee.PrincipalSplit;

                    if (processingFee.PaymentGatewaySplit > 0)
                    {
                        principalProcSplit = principalProcSplit + processingFee.PaymentGatewaySplit;
                    }
                }

                var principalSplit = principalExpSplit + principalProcSplit;
                if (principalSplit < 1)
                {
                    return new Lineitem[] { };
                }

                var principalDetails = GetDprDetails(principalSplit.ToString(CultureInfo.InvariantCulture));
                list.Add(principalDetails);

                var vendorSplit = vendorProcSplit + vendorExpSplit;
                if (vendorSplit > 0)
                {
                    var vendoDetails = GetMaxfrontDetails(vendorSplit.ToString(CultureInfo.InvariantCulture));
                    list.Add(vendoDetails);
                }

                return list.ToArray();
            }
            catch (Exception ex)
            {
                ErrorLogger.LoggError(ex.StackTrace, ex.Source, ex.Message);
                return new Lineitem[] { };
            }
        }


        public Lineitem[] ComputeExpenditionaryPaymentSplit(FeeObject expenditionaryfee)
        {
            try
            {
                if (expenditionaryfee == null)
                {
                    return new Lineitem[] { };
                }

                var list = new List<Lineitem>();

                var principalExpSplit = 0.0;
                var vendorExpSplit = 0.0;

                if (expenditionaryfee.VendorSplit > 0)
                {
                    vendorExpSplit = expenditionaryfee.VendorSplit;
                }

                principalExpSplit = expenditionaryfee.PrincipalSplit;

                if (expenditionaryfee.PaymentGatewaySplit > 0)
                {
                    principalExpSplit = principalExpSplit + expenditionaryfee.PaymentGatewaySplit;
                }

                var principalSplit = principalExpSplit;
                if (principalSplit < 1)
                {
                    return new Lineitem[] { };
                }

                var principalDetails = GetDprDetails(principalSplit.ToString(CultureInfo.InvariantCulture));
                list.Add(principalDetails);

                var vendorSplit = vendorExpSplit;
                if (vendorSplit > 0)
                {
                    var vendoDetails = GetMaxfrontDetails(vendorSplit.ToString(CultureInfo.InvariantCulture));
                    list.Add(vendoDetails);
                }

                return list.ToArray();
            }
            catch (Exception ex)
            {
                ErrorLogger.LoggError(ex.StackTrace, ex.Source, ex.Message);
                return new Lineitem[] { };
            }
        }

        public string GetHash(string data)
        {
            return BitConverter.ToString(new SHA512CryptoServiceProvider().ComputeHash(Encoding.Default.GetBytes(data))).Replace("-", String.Empty).ToLower();
        }

        #region Get Split details
        public Lineitem GetFgDetails(string amount)
        {
            return new Lineitem
            {
                LineItemsId = "01",
                BeneficiaryName = "Petroleum Inspectorate",
                BeneficiaryAccount = "3000023072",
                BankCode = "000",
                BeneficiaryAmount = amount,
                DeductFeeFrom = 0
            };
        }

        public Lineitem GetDprDetails(string amount)
        {
            return new Lineitem
            {
                LineItemsId = "02",
                BeneficiaryName = "Department Of Petroleum Resources",
                BeneficiaryAccount = "0006228731",
                BankCode = "058",
                BeneficiaryAmount = amount,
                DeductFeeFrom = 1
            };
        }

        public Lineitem GetMaxfrontDetails(string amount)
        {
            return new Lineitem
            {
                LineItemsId = "03",
                BeneficiaryName = "Maxfront",
                BeneficiaryAccount = "0014660703",
                BankCode = "039",
                BeneficiaryAmount = amount,
                DeductFeeFrom = 0
            };
        }
        #endregion
    }
}