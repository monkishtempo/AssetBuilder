
namespace NaturalLanguageWizard
{
    interface ISaveable<T> : IRestorable
    {
        void SaveAndInitialise(T initialValue);
    }

    interface ISaveable : IRestorable
    {
        void SaveAndClear();        
    }

    interface IRestorable
    {
        void RestoreLast();
    }
}
