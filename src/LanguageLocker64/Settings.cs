namespace Vurdalakov.LanguageLocker64
{
    using System;
    using System.ComponentModel;

    public class Settings : BaseOptionModel<Settings>
    {
        [DefaultValue(true)]
        public Boolean LockLanguage { get; set; } = true;
    }
}
