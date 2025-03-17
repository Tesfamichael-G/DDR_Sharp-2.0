namespace SimulatorLib
{


    public class DRAM_Specification
    {

        public string ID;

        //Linking ...
        public string DDR_TYPE;
        public string ORG_ID;
        public string DDR_ID;


        public Organization ORG ;
        public Power POWER ;
        public Timing TIMING ;


        ///********* DRAM ORGANIZATION *********
        public class Organization
        {
            public int DQ ;
            public int SIZE ;
            public int Channel ;
            public int Rank ;
            public int BankGroup ;
            public int Bank ;
            public int Row ;
            public int Column ;

        }

        ///********* POWER PARAMETERS *********
        public class Power
        {
            public float IDD0 ;
            public float IDD02 ;
            public float IDD2P0 ;
            public float IDD2P1 ;
            public float IDD2P ;
            public float IDD2N ;
            public float IDD3P ;
            public float IDD3P0 ;
            public float IDD3P1 ;
            public float IDD3N ;
            public float IDD4R ;
            public float IDD4W ;
            public float IDD5 ;
            public float IDD6 ;
            public float IDD62 ;
            public float VDD ;
            public float VDD2 ;
        }


        ///********* TIMING PARAMETERS *********
        public class Timing
        {
            public int RATE ;
            public float FREQ ;
            public float TCK ;

            public  int BL ;
            public  int CL ;
           //public int  CCD ;
            public  int CWL ;
            public  int CKESR ;
            public  int FAW ;
            public  int RAS ;
            public  int RC ;
            public  int RCD ;
            public  int RFC ;
            public  int REFI ;
            public  int RP ;
           //public int  RRD ;
            public  int RTP ;
            public  int RTRS ;
            public  int WR ;
           //public int  WTR ;
            public  int PD ;
            public  int XP ;
            public  int XPDLL ;
            public  int XS ;
            public  int XSDLL ;
            public  int CCDS ;
            public  int RRDS ;
            public  int WTRS ;
            public  int CCDL ;
            public  int RRDL ;
            public  int WTRL ;
        }

    }




}
