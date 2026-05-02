using System;

namespace Restless.Dream
{
    public interface IExtractionMinigame
    {
        void Begin(Action onSuccess, Action onFailure);
        void Cancel();
        bool IsActive { get; }
    }
}
