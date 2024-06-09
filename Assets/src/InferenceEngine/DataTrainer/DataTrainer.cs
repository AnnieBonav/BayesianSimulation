using System.Collections.Generic;
using UnityEngine;

public abstract class DataTrainer
{
    protected DATA_TRAINER_TYPE dataTrainerType;
    public DATA_TRAINER_TYPE DataTrainerType => dataTrainerType;
    public abstract ACTIVITY_TYPE ChooseTrainingActivity(List<ACTIVITY_TYPE> availableActivities, InferenceData trainingStateValues);
    public static DataTrainer CreateDataTrainer(DATA_TRAINER_TYPE dataTrainerType)
    {
        switch (dataTrainerType)
        {
            case DATA_TRAINER_TYPE.RANDOM:
                return new RandomTrainer();
            case DATA_TRAINER_TYPE.HEURISTICS:
                return new HeuristicsTrainer();
            default:
                throw new System.Exception("Invalid Data Trainer type");
        }
        
    }
}
