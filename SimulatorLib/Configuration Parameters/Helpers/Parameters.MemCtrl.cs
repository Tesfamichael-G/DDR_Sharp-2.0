using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using SimulatorLib.Common;

namespace SimulatorLib
{

    public enum REF_MODE
    {
        RANK,
        BANK,
        NONE
    }


    public enum PageMappingMethod
    {
        CONTIGIOUS,
        RANDOM,
        SEQUENCE
    }


    public partial class MemCtrl_Parameters
    {

        #region MemCtrl

        public PageMappingMethod MAPPING_METHOD { get; set; } = PageMappingMethod.RANDOM;
        public TranslationMethod ADDRESS_MAPPING { get; set; } = TranslationMethod.ROW_RANK_BANK_CHAN_COL;//TranslationMethod.ROW_BANK_RANK_COL_CHAN;

        public int WQ_LOOKUP_LATENCY   = 10;

        public int READQ_MAX { get; set; } = 64;
        public int WRITEQ_MAX { get; set; } = 64;
        public bool OPEN_PAGE_POLICY { get; set; } = true;
        public int XBAR_LATENCY { get; set; } = 16;
        public bool INCLUDE_XBAR_LATENCY { get; set; } = true;
        public REF_MODE REFRESH_MODE { get; set; } = REF_MODE.RANK;

        #endregion


        private SpecInfo spec;
        public SpecInfo Spec
        {
            get
            {
                if (spec == null)
                    spec = new SpecInfo(this);

                return spec;
            }
        }

        public class SpecInfo
        {

            public List<KeyValue_Item> MemCtrl;
            PropertyInfo[] properties;

            public SpecInfo(MemCtrl_Parameters param)
            {
                if (MemCtrl == null)
                    MemCtrl = new List<KeyValue_Item>();

                properties = param.GetType().GetProperties();

                System.Diagnostics.Debug.Print($"properties.count = {properties.Count()}");

                foreach (var prop in properties)
                {
                    if (prop.Name == "Spec")
                        continue;

                    var val = prop.GetValue(param);
                    var name = prop.Name + ": ";

                    var line = new KeyValue_Item { Name = name.ToUpper(), Value = val };
                    MemCtrl.Add(line);


                }

            }


        }




    }


}
