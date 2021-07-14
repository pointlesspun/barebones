using UnityEngine.InputSystem;

namespace BareBones.Common
{
    public interface IAgentController
    {
        bool IsActive { get; }

        void SetActive(bool value);

        void ChangeFiringState(InputAction.CallbackContext context);
        void FireWeapons();
        void UpdateDirection(InputAction.CallbackContext context);
    }
}