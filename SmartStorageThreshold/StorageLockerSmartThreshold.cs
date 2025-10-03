using System.Reflection;
using HarmonyLib;
using KSerialization;
using STRINGS;
using UnityEngine;

namespace SmartStorageThreshold
{
    public class StorageLockerSmartThreshold : KMonoBehaviour, IActivationRangeTarget
    {
        [Serialize]
        private float _activateValue = 100f;

        [Serialize]
        private float _deactivateValue;

        [Serialize]
        private bool _lastSignal = false;

        private StorageLockerSmart _storageLockerSmart;
        private MethodInfo _updateLogicCircuitMethod;

        // IActivationRangeTarget implementation
        // Actually holds deactivate threshold
        public float ActivateValue
        {
            get => _activateValue;
            set
            {
                _activateValue = value;
                _lastSignal = true;
                InvokeUpdateLogicCircuitCB();
            }
        }

        // Actually holds active threshold
        public float DeactivateValue
        {
            get => _deactivateValue;
            set
            {
                _deactivateValue = value;
                _lastSignal = false;
                InvokeUpdateLogicCircuitCB();
            }
        }

        // Evaluate mass against thresholds with simple hysteresis:
        public bool EvaluateMass(float percent)
        {
            bool newSignal = _lastSignal;
            float currentValue = percent * 100f;
            if (currentValue >= ActivateValue)
                newSignal = false;
            else if (currentValue <= DeactivateValue)
                newSignal = true;

            _lastSignal = newSignal;
            return newSignal;
        }

        public float MinValue => 0f;
        public float MaxValue => 100f;

        public bool UseWholeNumbers => true;

        public string ActivateTooltip => BUILDINGS.PREFABS.SMARTRESERVOIR.DEACTIVATE_TOOLTIP;

        public string DeactivateTooltip => BUILDINGS.PREFABS.SMARTRESERVOIR.ACTIVATE_TOOLTIP;

        public string ActivationRangeTitleText => BUILDINGS.PREFABS.SMARTRESERVOIR.SIDESCREEN_TITLE;

        public string ActivateSliderLabelText =>
            BUILDINGS.PREFABS.SMARTRESERVOIR.SIDESCREEN_DEACTIVATE;

        public string DeactivateSliderLabelText =>
            BUILDINGS.PREFABS.SMARTRESERVOIR.SIDESCREEN_ACTIVATE;

        protected override void OnSpawn()
        {
            base.OnSpawn();
            _storageLockerSmart = gameObject.GetComponent<StorageLockerSmart>();

            // find the private UpdateLogicCircuitCB(object) method via reflection on StorageLockerSmart
            if (_storageLockerSmart != null)
            {
                _updateLogicCircuitMethod = typeof(StorageLockerSmart).GetMethod(
                    "UpdateLogicCircuitCB",
                    BindingFlags.Instance | BindingFlags.NonPublic
                );
            }
        }

        private static readonly EventSystem.IntraObjectHandler<StorageLockerSmartThreshold> OnCopySettingsDelegate =
            new EventSystem.IntraObjectHandler<StorageLockerSmartThreshold>(
                delegate(StorageLockerSmartThreshold component, object data)
                {
                    component.OnCopySettings(data);
                }
            );

        protected override void OnPrefabInit()
        {
            base.OnPrefabInit();
            Subscribe(-905833192, OnCopySettingsDelegate);
        }

        private void OnCopySettings(object data)
        {
            StorageLockerSmartThreshold component = (
                (GameObject)data
            ).GetComponent<StorageLockerSmartThreshold>();
            if (component == null)
                return;
            _activateValue = component._activateValue;
            _deactivateValue = component._deactivateValue;
        }

        private void InvokeUpdateLogicCircuitCB()
        {
            if (_storageLockerSmart == null || _updateLogicCircuitMethod == null)
                return;

            try
            {
                // The original method signature is UpdateLogicCircuitCB(object data)
                _updateLogicCircuitMethod.Invoke(_storageLockerSmart, new object[] { null });
            }
            catch (TargetInvocationException tie)
            {
                Debug.LogError($"Error invoking UpdateLogicCircuitCB: {tie}");
            }
            catch (System.Exception e)
            {
                Debug.LogError($"Error invoking UpdateLogicCircuitCB: {e}");
            }
        }
    }
}
