/*==============================================================================
Copyright (c) 2021 PTC Inc. All Rights Reserved.

Vuforia is a trademark of PTC Inc., registered in the United States and other
countries.
==============================================================================*/

using Vuforia;

public class MTAdvancedObserverEventHandler : MTObserverEventHandler
{
    ModelTargetsManager mModelTargetsManager;
    bool mIsModelTargetReferenceStored;

    protected override void Start()
    {
        base.Start();
        mModelTargetsManager = FindObjectOfType<ModelTargetsManager>();
    }

    void Update()
    {
        if (!mIsModelTargetReferenceStored && mObserverBehaviour != null)
        {
            VLog.Log("yellow", "Storing reference to " + mObserverBehaviour.TargetName + ": " + 
                               mObserverBehaviour.TargetStatus.Status + " -- " + mObserverBehaviour.TargetStatus.StatusInfo);

            var modelTargetBehaviour = mObserverBehaviour as ModelTargetBehaviour;

            if (modelTargetBehaviour != null && mModelTargetsManager)
            {
                mModelTargetsManager.AddAdvancedModelTargetBehaviour(modelTargetBehaviour);
                mIsModelTargetReferenceStored = true;
            }
        }
    }
    
    protected override void OnTrackingFound()
    {
        base.OnTrackingFound();
        mModelTargetsManager.EnableSymbolicTargetsUI(false);
    }
}