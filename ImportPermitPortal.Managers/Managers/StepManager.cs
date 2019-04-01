using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using ImportPermitPortal.DataObjects;
using ImportPermitPortal.DataObjects.Helpers;
using ImportPermitPortal.DynamicMapper.DynamicMapper;
using ImportPermitPortal.EF.Model;

namespace ImportPermitPortal.Managers.Managers
{
    public class StepManager
    {

        public long AddStep(StepObject step)
        {
            try
            {
                if (step == null)
                {
                    return -2;
                }

              

                using (var db = new ImportPermitEntities())
                {

                    var steps = db.Steps.Where(s => s.Process.ImportStage.Id == step.ImportStageId).ToList();

                    var count = steps.Count();
                    var counter = count + 1;

                    var previousStepSequence = step.PreviousStepSequence;
                    if (count == 0)
                    {
                        step.SequenceNumber = 1;
                        var stepEntity = ModelMapper.Map<StepObject, Step>(step);
                        if (stepEntity == null || string.IsNullOrEmpty(stepEntity.Name))
                        {
                            return -2;
                        }

                        var returnStatus = db.Steps.Add(stepEntity);
                        db.SaveChanges();
                        return returnStatus.Id;
                    }
                    else
                    {
                        foreach (var item in steps)
                        {
                            if (item.SequenceNumber > previousStepSequence)
                            {
                                item.SequenceNumber = item.SequenceNumber + 1;
                                db.Entry(item).State = EntityState.Modified;
                            }
                        }

                        step.SequenceNumber = previousStepSequence + 1;
                        var stepEntity = ModelMapper.Map<StepObject, Step>(step);
                        if (stepEntity == null || string.IsNullOrEmpty(stepEntity.Name))
                        {
                            return -2;
                        }
                        db.Steps.Add(stepEntity);



                        db.SaveChanges();
                        return 1;

                    }
                   

                }
            }
            catch (Exception ex)
            {
                ErrorLogger.LoggError(ex.StackTrace, ex.Source, ex.Message);
                return 0;
            }
        }

        public long UpdateStep(StepObject step)
        {
            try
            {
                if (step == null)
                {
                    return -2;
                }

                var stepEntity = ModelMapper.Map<StepObject, Step>(step);
                if (stepEntity == null || stepEntity.Id < 1)
                {
                    return -2;
                }

                using (var db = new ImportPermitEntities())
                {
                    db.Steps.Attach(stepEntity);
                    db.Entry(stepEntity).State = EntityState.Modified;
                    db.SaveChanges();
                    return step.Id;
                }
            }
            catch (Exception ex)
            {
                ErrorLogger.LoggError(ex.StackTrace, ex.Source, ex.Message);
                return 0;
            }
        }
        public List<StepObject> GetSteps()
        {
            try
            {
                using (var db = new ImportPermitEntities())
                {
                    var steps = db.Steps.ToList();
                    if (!steps.Any())
                    {
                        return new List<StepObject>();
                    }
                    var objList = new List<StepObject>();
                    steps.ForEach(app =>
                    {
                        var stepObject = ModelMapper.Map<Step, StepObject>(app);
                        if (stepObject != null && stepObject.Id > 0)
                        {
                            objList.Add(stepObject);
                        }
                    });

                    return !objList.Any() ? new List<StepObject>() : objList;
                }
            }
            catch (Exception ex)
            {
                ErrorLogger.LoggError(ex.StackTrace, ex.Source, ex.Message);
                return null;
            }
        }




       


        public List<StepObject> GetSteps(int? itemsPerPage, int? pageNumber, out int countG)
        {
            try
            {
                if ((itemsPerPage != null && itemsPerPage > 0) && (pageNumber != null && pageNumber >= 0))
                {
                    var tpageNumber = (int)pageNumber;
                    var tsize = (int)itemsPerPage;

                    using (var db = new ImportPermitEntities())
                    {
                        var steps =
                            db.Steps.Where(m=>m.SequenceNumber != 0).OrderByDescending(m => m.Id)
                                .Skip(tpageNumber).Take(tsize)
.Include("Process").Include("Group")
                                .ToList();
                        if (steps.Any())
                        {
                            var newList = new List<StepObject>();
                            steps.ForEach(app =>
                            {
                                var stepObject = ModelMapper.Map<Step, StepObject>(app);
                                if (stepObject != null && stepObject.Id > 0)
                                {
                                    stepObject.ProcessName = app.Process.Name;
                                    stepObject.GroupName = app.Group.Name;
                                    stepObject.ImportStageName = app.Process.ImportStage.Name;
                                    newList.Add(stepObject);
                                }
                            });
                            countG = db.Steps.Count();
                            return newList;
                        }
                    }

                }
                countG = 0;
                return new List<StepObject>();
            }
            catch (Exception ex)
            {
                countG = 0;
                return new List<StepObject>();
            }
        }


        public StepObject GetStep(long stepId)
        {
            try
            {
                using (var db = new ImportPermitEntities())
                {
                    var steps =
                        db.Steps.Where(m => m.Id == stepId)
                            .ToList();
                    if (!steps.Any())
                    {
                        return new StepObject();
                    }

                    var app = steps[0];
                    var stepObject = ModelMapper.Map<Step, StepObject>(app);
                    if (stepObject == null || stepObject.Id < 1)
                    {
                        return new StepObject();
                    }

                    stepObject.ImportStageId = app.Process.ImportStageId;
                    stepObject.ImportStageName = app.Process.ImportStage.Name;
                    stepObject.ProcessName = app.Process.Name;
                    stepObject.GroupName = app.Group.Name;
                    stepObject.ActivityTypeName = app.StepActivityType.Name;

                    var previousStepSeq = app.SequenceNumber - 1;

                    var previousStep = db.Steps.Where(s => s.SequenceNumber == previousStepSeq).ToList();

                    if (previousStep.Any())
                    {
                        stepObject.PreviousStepId = previousStep[0].Id;
                        stepObject.PreviousStepName = previousStep[0].Name;
                        stepObject.ExpectedDeliveryDurationStr = previousStep[0].ExpectedDeliveryDuration.ToString();

                    }

                   
                    return stepObject;
                }
            }
            catch (Exception ex)
            {
                ErrorLogger.LoggError(ex.StackTrace, ex.Source, ex.Message);
                return new StepObject();
            }
        }

        public List<StepObject> Search(string searchCriteria)
        {
            try
            {
                using (var db = new ImportPermitEntities())
                {
                    var steps =
                        db.Steps.Where(m => m.Name.ToLower().Trim() == searchCriteria.ToLower().Trim())
                        .ToList();

                    if (steps.Any())
                    {
                        var newList = new List<StepObject>();
                        steps.ForEach(app =>
                        {
                            var stepObject = ModelMapper.Map<Step, StepObject>(app);
                            if (stepObject != null && stepObject.Id > 0)
                            {
                                newList.Add(stepObject);
                            }
                        });

                        return newList;
                    }
                }
                return new List<StepObject>();
            }

            catch (Exception ex)
            {
                ErrorLogger.LoggError(ex.StackTrace, ex.Source, ex.Message);
                return new List<StepObject>();
            }
        }

        public long DeleteStep(long stepId)
        {
            try
            {
                using (var db = new ImportPermitEntities())
                {
                    var myItems =
                        db.Steps.Where(m => m.Id == stepId).ToList();
                    if (!myItems.Any())
                    {
                        return 0;
                    }

                    var item = myItems[0];
                    var seq = item.SequenceNumber;
                    var processId = item.ProcessId;
                    var process = db.Processes.Find(processId);
                    var stageId = process.ImportStageId;

                    var affectedSteps = db.Steps.Where(s => s.Process.ImportStageId == stageId);

                    foreach (var affectedItem in affectedSteps)
                    {
                        if (affectedItem.SequenceNumber > seq)
                        {
                            affectedItem.SequenceNumber = affectedItem.SequenceNumber - 1;

                            db.Steps.Attach(affectedItem);
                            db.Entry(affectedItem).State = EntityState.Modified;
                        }
                    }

                    item.SequenceNumber = 0;
                    db.Steps.Attach(item);
                    db.Entry(item).State = EntityState.Modified;
                    db.SaveChanges();
                    return 5;

                    
                }
            }
            catch (Exception ex)
            {
                ErrorLogger.LoggError(ex.StackTrace, ex.Source, ex.Message);
                return 0;
            }
        }
    }
}
