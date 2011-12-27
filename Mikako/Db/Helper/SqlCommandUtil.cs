using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Data.Common;

namespace Com.Luxiar.Mikako.Db
{
    static class DbCommandUtil
    {
        public static int Execute(IDbCommand cmd)
        {
            try
            {
                return cmd.ExecuteNonQuery();
            }
            catch (SystemException e)
            {

                throw MakeException(e, cmd);
            }
        }

        public static object ExecuteScalar(IDbCommand cmd)
        {
            try
            {
                return cmd.ExecuteScalar();
            }
            catch (SystemException e)
            {
                throw MakeException(e, cmd);
            }
        }

        public static List<DataRowAccessor> SelectFromDataAdapter(IDbDataAdapter adapter)
        {
            return SelectToDataRowList(adapter).ConvertAll<DataRowAccessor>(delegate(DataRow row) { return new DataRowAccessor(row); });
        }

        public static DataSet SelectFromDataAdapterDataSet(IDbDataAdapter adapter)
        {
            DataSet ds = new DataSet();
            FillDataSet(adapter, ds);
            return ds;
        }

        //Select���ʂ�DataRow�̃��X�g�ŕԋp���܂��B
        //�����e�[�u���͖��Ή��ł��B
        private static List<DataRow> SelectToDataRowList(IDbDataAdapter adapter)
        {
            using (DataSet ds = new DataSet())
            {
                List<DataRow> ret = new List<DataRow>();

                if (0 >= FillDataSet(adapter, ds))
                {
                    return ret;
                }

                //DataRowCollection��List�ɋl�ߑւ���B
                foreach (DataRow row in ds.Tables[0].Rows)
                {
                    ret.Add(row);
                }
                return ret;
            }
        }

        private static int FillDataSet(IDbDataAdapter adapter, DataSet ds)
        {
            try
            {
                return adapter.Fill(ds);
            }
            catch (SystemException e)
            {
                throw MakeException(e, adapter.SelectCommand);
            }
        }

        private static ApplicationException MakeException(SystemException e, IDbCommand cmd)
        {
            return new ApplicationException(e.Message + "\n" + cmd.CommandText, e);
        }
    }
}
