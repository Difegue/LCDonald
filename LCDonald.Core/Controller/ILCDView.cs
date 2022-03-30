using LCDonald.Core.Model;
using System.Collections.Generic;

namespace LCDonald.Core.Controller
{
    /// <summary>
    /// Interface for the view of a LCD Game -- to be implemented by the not-NET Standard portion of the code.
    /// </summary>
    public interface ILCDView
    {
        List<LCDGameInput> GetPressedInputs();
        void PlaySounds(List<LCDGameSound> soundsToPlay);
    }
}