namespace CoppyWeeklyMonthly.Common
{
    class SQLRequests
    {
        public string SQL_GetVhodID(string CurrendDate)
        {
            return @"select f.name_file,v.INP_N, m.path||'\'||floor((f.id_vpop-50000)/10000)*10000||'\'||floor(mod((f.id_vpop-50000),10000)/100)*100||'\'||mod(mod((f.id_vpop-50000),10000),100)||'\'||f.name_file files,f.id_vpop idv  from tronix.tp_file f,tronix.tp_main m, tronix.tp_vhod v  where m.tp_id = f.id_tip and v.tp_input_id = f.id_vpop and v.id_tip = f.id_tip and v.tp_input_id in 
(select v.tp_input_id from tronix.tp_vhod v
where v.data_i>= add_months(to_date('" + CurrendDate+ @"','MM.YYYY'),-1)
and f.name_file is not null
and (v.tp_input_id,v.id_tip) in
(select f.id_vpop,f.id_tip from tronix.tp_file f))";
        }

        public string SQL_GetIshodID(string CurrendDate)
        {
            return @"select f.name_file,i.isx_n as INP_N, m.path||'\'||floor((f.id_vpop-50000)/10000)*10000||'\'||floor(mod((f.id_vpop-50000),10000)/100)*100||'\'||mod(mod((f.id_vpop-50000),10000),100)||'\'||f.name_file files,f.id_vpop idv from tronix.tp_file f,tronix.tp_main m,tronix.tp_ishod i where i.id_tip=f.id_tip and i.tp_export_id = f.id_vpop and m.tp_id=f.id_tip and i.tp_export_id in
(select i.tp_export_id from tronix.tp_ishod i
where i.date1>=add_months(to_date('" + CurrendDate + @"','MM.YYYY'),-1)
and f.name_file is not null
and (i.tp_export_id,i.id_tip) in
(select f.id_vpop,f.id_tip from tronix.tp_file f))";
        }
    }
}
