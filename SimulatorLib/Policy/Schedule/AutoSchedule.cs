//using SimulatorLib.CPU;

//namespace SimulatorLib.Memory.Schedule
//{
//    public class AutoSchedule : IScheduler
//    {

//        private MemoryController MC;

//        int NUM_RANKS;
//        int NUM_GROUPS;
//        int NUM_BANKS;
//        public AutoSchedule(MemoryController mc)
//        {
//            MC = mc;
//            NUM_RANKS = MC.PARAM.NUM_RANKS;// MC.CHANNEL.Ranks.Count;
//            NUM_GROUPS = MC.PARAM.NUM_BANK_GROUPS;// MC.CHANNEL.Ranks[0].Banks[0].Count;
//            NUM_BANKS = MC.PARAM.NUM_BANKS;
//        }

//        public void Tick()
//        {
//            if (MC.WRITE_MODE.DrainWrites)
//                IssueWriteRequests();
//            else
//                IssueReadRequests();
//        }



//        private void IssueReadRequests()
//        {
//            DEBUG.Assert(!MC.WRITE_MODE.DrainWrites);

//            for (int r = 0; r < NUM_RANKS; r++)
//            {
//                for (int g = 0; g < NUM_GROUPS; g++)
//                    for (int b = 0; b < NUM_BANKS;)
//                    {
//                        int count = MC.QUEUE.READS[r][g][b].Count;
//                        for (int i = 0; i < count; i++)
//                        {
//                            var request = MC.QUEUE.READS[r][g][b][i];
//                            if (!request.IsServed && request.CanBeIssued)
//                            {
//                                MC.ISSUE(request);
//                                break;
//                            }

//                        }
//                    }
//            }

//        }
//        private void IssueWriteRequests()
//        {
//            DEBUG.Assert(MC.WRITE_MODE.DrainWrites);

//            for (int r = 0; r < NUM_RANKS; r++)
//            {
//                for (int g = 0; g < NUM_GROUPS; g++)
//                {
//                    for (int b = 0; b < NUM_BANKS;)
//                    {
//                        int count = MC.QUEUE.WRITES[r][g][b].Count;
//                        for (int i = 0; i < count; i++)
//                        {
//                            var request = MC.QUEUE.WRITES[r][g][b][i];
//                            if (!request.IsServed && request.CanBeIssued)
//                            {
//                                MC.ISSUE(request);
//                                break;
//                            }
//                        }
//                    }

//                }
//            }

//        }

//        //private void IssueReadRequests()
//        //{
//        //    DEBUG.Assert(!MC.WRITE_MODE.DrainWrites);

//        //    for (int r = 0; r < NUM_RANKS; r++)
//        //    {
//        //        for (int b = 0; b < NUM_BANKS; b++)
//        //        {
//        //            int count = MC.QUEUE.READS[r, b].Count;
//        //            for (int i = 0; i < count; i++)
//        //            {
//        //                var request = MC.QUEUE.READS[r, b][i];
//        //                if (!request.IsServed && request.CanBeIssued)
//        //                {
//        //                    MC.ISSUE(request);
//        //                    break;
//        //                }

//        //            }
//        //        }
//        //    }

//        //}

//        //private void IssueWriteRequests()
//        //{
//        //    DEBUG.Assert(MC.WRITE_MODE.DrainWrites);

//        //    for (int r = 0; r < NUM_RANKS; r++)
//        //    {
//        //        for (int b = 0; b < NUM_BANKS; b++)
//        //        {
//        //            int count = MC.QUEUE.WRITES[r, b].Count;
//        //            for (int i = 0; i < count; i++)
//        //            {
//        //                var request = MC.QUEUE.WRITES[r, b][i];
//        //                if (!request.IsServed && request.CanBeIssued)
//        //                {
//        //                    MC.ISSUE(request);
//        //                    break;
//        //                }

//        //            }
//        //        }
//        //    }

//        //}
//        //


//    }
//}