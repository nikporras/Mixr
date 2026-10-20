using Mixr.Actions.DataModel.Serialization;

namespace Mixr.Actions.DataModel
{
    interface IPartWithApp
    {
        AppRef App { get; set; }
    }
}
