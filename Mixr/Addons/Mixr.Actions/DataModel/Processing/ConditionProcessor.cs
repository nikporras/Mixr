using Mixr.Actions.DataModel.Enum;
using Mixr.Actions.DataModel.Serialization;
using System;
using Mixr.DataModel.WindowsAudio;

namespace Mixr.Actions.DataModel.Processing
{
    class ConditionProcessor
    {
        public static bool IsMet(BaseCondition condition)
        {
            if (condition is ProcessCondition)
            {
                bool isProcessRunning = ProcessWatcher.Current.IsRunning(((ProcessCondition)condition).Text);
                switch (((ProcessCondition)condition).Option)
                {
                    case ProcessStateKind.Running:
                        return isProcessRunning;
                    case ProcessStateKind.NotRunning:
                        return !isProcessRunning;
                    default:
                        throw new NotImplementedException();
                }
            }
            else if (condition is DefaultDeviceCondition)
            {
                var mgr = WindowsAudioFactory.Create((AudioDeviceKind)System.Enum.Parse(typeof(AudioDeviceKind), ((DefaultDeviceCondition)condition).Device.Kind));

                var isDeviceCurrentlyDefault = ((DefaultDeviceCondition)condition).Device.Id == mgr.Default?.Id;
                switch (((DefaultDeviceCondition)condition).Option)
                {
                    case ComparisonBoolKind.Is:
                        return isDeviceCurrentlyDefault;
                    case ComparisonBoolKind.IsNot:
                        return !isDeviceCurrentlyDefault;
                    default:
                        throw new NotImplementedException();
                }
            }
            else if (condition is VariableCondition)
            {
                return (MixrActionsAddon.Current.LocalVariables[((VariableCondition)condition).Text] == (((VariableCondition)condition).Value == BoolValue.True));
            }
            throw new NotImplementedException();
        }
    }
}
