using System;

namespace Common
{
    public class SomeSkjemaDataContract
    {
        public Guid FormGuid { get; set; }
        public Guid SomePatientGuid { get; set; }
        public SomeEnumDataContract SomeEnumValue { get; set; }
        public bool SomeBoolean { get; set; }
        public DateTime SomeDateTime { get; set; }
    }
}
