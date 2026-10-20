using Mixr.Actions.DataModel;
using Mixr.Actions.DataModel.Processing;
using Mixr.Actions.DataModel.Serialization;
using Mixr.Actions.ViewModel;
using Mixr.DataModel.Storage;
using Mixr.Extensibility;
using Mixr.UI.Helpers;
using Mixr.UI.ViewModels;
using System;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.IO;
using System.Linq;

namespace Mixr.Actions
{
    [Export(typeof(MixrAddon))]
    public class MixrActionsAddon : MixrAddon, IMixrAddonEvents, IMixrAddonSettingsPage, IMixrAddonNotificationAreaContextMenu
    {
        public static MixrActionsAddon Current { get; private set; }
        public LocalVariablesContainer LocalVariables { get; private set; }

        public MixrActionsAddon() : base()
        {
            DisplayName = Properties.Resources.MyActionsText;
        }

        public MixrAction[] Actions
        {
            get => _actions;
            set
            {
                Settings.Set(c_actionsSettingKey, value);
                LoadAndRegister();
            }
        }

        private readonly string c_actionsSettingKey = "ActionsData";
        private MixrAction[] _actions = new MixrAction[] { };
        private TriggerManager _triggerManager = new TriggerManager();

        public void OnAddonEvent(AddonEventKind evt)
        {
            if (evt == AddonEventKind.AddonsInitialized)
            {
                Current = this;
                LocalVariables = new LocalVariablesContainer(Settings);

                _triggerManager.Triggered += OnTriggered;
                LoadAndRegister();

                _triggerManager.OnEvent(AddonEventKind.InitializeAddon);
            }
            else if (evt == AddonEventKind.AppShuttingDown)
            {
                _triggerManager.OnEvent(AddonEventKind.AppShuttingDown);
            }
        }

        public SettingsCategoryViewModel GetSettingsCategory()
        {
            LoadAddonResources();
            return new ActionsCategoryViewModel();
        }

        public IEnumerable<ContextMenuItem> NotificationAreaContextMenuItems
        {
            get
            {
                var ret = new List<ContextMenuItem>();

                if (MixrActionsAddon.Current == null)
                {
                    return ret;
                }

                foreach (var item in MixrActionsAddon.Current.Actions.Where(a => a.Triggers.FirstOrDefault(ax => ax is ContextMenuTrigger) != null))
                {
                    ret.Add(new ContextMenuItem
                    {
                        Glyph = "\xE1CE",
                        IsChecked = true,
                        DisplayName = item.DisplayName,
                        Command = new RelayCommand(() => MixrActionsAddon.Current.TriggerAction(item))
                    });
                }
                return ret;
            }
        }

        private void LoadAndRegister()
        {
            _triggerManager.Clear();
            _actions = Settings.Get(c_actionsSettingKey, new MixrAction[] { });
            _actions.SelectMany(a => a.Triggers).ToList().ForEach(t => _triggerManager.Register(t));
        }

        public void Import(string fileName)
        {
            var imported = Serializer.FromString<MixrAction[]>(File.ReadAllText(fileName)).ToList();
            foreach(var imp in imported)
            {
                imp.Id = Guid.NewGuid();
            }
            imported.AddRange(Actions);
            Actions = imported.ToArray();
        }

        public string Export()
        {
            return Settings.Get(c_actionsSettingKey, "");
        }

        private void OnTriggered(BaseTrigger trigger)
        {
            var action = Actions.FirstOrDefault(a => a.Triggers.Contains(trigger));
            if (action != null && action.Conditions.All(c => ConditionProcessor.IsMet(c)))
            {
                TriggerAction(action);
            }
        }

        public void TriggerAction(MixrAction action)
        {
            action.Actions.ToList().ForEach(a => ActionProcessor.Invoke(a));
        }
    }
}