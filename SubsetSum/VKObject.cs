
#region Using directives
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.IO;
using System.Security.Cryptography;
using System.Runtime.InteropServices;
#endregion


namespace SubsetSum
{
    #region Interfaces

    /// <summary>
    /// The public interface describing the COM interface of the coclass 
    /// </summary>
    [Guid("C999E822-F213-40FE-BE39-FBD91D542B2E")]          // IID
    [ComVisible(true)]
    // Dual interface by default. This allows the client to get the best of 
    // both early binding and late binding.
    //[InterfaceType(ComInterfaceType.InterfaceIsDual)]
    //[InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
    //[InterfaceType(ComInterfaceType.InterfaceIsIDispatch)]
    public interface IVKObject
    {
        #region Properties

        bool lastresult { get; set;}

        #endregion

        #region Methods

        int[] getSolution(int[] parameters, int findingSumm, int timeout);
        int[] getRandomList(int size, int min, int max);
        int getRandomInt(int min, int max);
      

        #endregion
    }

    /// <summary>
    /// The public interface describing the events the coclass can sink
    /// </summary>
    [Guid("549525DD-F5D7-4013-B365-77137D8943F6")]
    [InterfaceType(ComInterfaceType.InterfaceIsIDispatch)]
    [ComVisible(true)]
    public interface IVKObjectEvents
    {
        #region Events

      
        #endregion
    }

    #endregion

    [ClassInterface(ClassInterfaceType.None)]           // No ClassInterface
    [ComSourceInterfaces(typeof(IVKObjectEvents))]
    [Guid("3CF1F67C-42B8-411C-8AC5-D793698F6F0B")]      // CLSID
    //[ProgId("CSCOMServerDll.CustomSimpleObject")]     // ProgID
    [ComVisible(true)]
    public class VKObject : IVKObject
    {
        #region Properties

        public bool lastresult
        {
            get;
            set;
        }

        #endregion

        #region Methods

      
        public int getRandomInt(int min, int max)
        {
            RNGCryptoServiceProvider rng = new RNGCryptoServiceProvider();
            byte[] buffer = new byte[4];
            rng.GetBytes(buffer);
            int result = BitConverter.ToInt32(buffer, 0);
            return new Random(result).Next(min, max);
        }


        private T Execute<T>(Func<T> func, int timeout)
        {
            T result;
            TryExecute(func, timeout, out result);
            return result;
        }

        private bool TryExecute<T>(Func<T> func, int timeout, out T result)
        {
            var t = default(T);
            var thread = new Thread(() => t = func());
            thread.Start();
            var completed = thread.Join(timeout);
            if (!completed)
                thread.Abort();
            result = t;
            return completed;
        }

        public int[] getRandomList(int size, int min, int max)
        {
            List<int> testList = new List<int>();

            for (int i = 0; i < size; i++)
            {
                testList.Add(getRandomInt(min, max));
            }
            return testList.ToArray();
        }


        public int[] getSolution(int[] parameters, int findingSumm, int timeout)
        {

            List<int> testList = new List<int>(parameters);
            var func = new Func<bool>(() =>
            {
                SubsetSum.FillDecisionTable(testList, findingSumm);
                return true;

            });
            bool result = false;
            TryExecute(func, timeout, out result);
            lastresult = result;
            SubsetSum.fixsum(findingSumm);
            return SubsetSum.GetLastResult(findingSumm).ToArray();

        }

        
        #endregion

        #region Events

      

        #endregion
    }

    static class SubsetSum
    {
        private static Dictionary<int, bool> memo;
        private static Dictionary<int, KeyValuePair<int, int>> prev;
        public static int tempsum = 0;

        static SubsetSum()
        {
            memo = new Dictionary<int, bool>();
            prev = new Dictionary<int, KeyValuePair<int, int>>();
        }

        public static void FillDecisionTable(List<int> inputArray, int sum)
        {
            memo.Clear();
            prev.Clear();

            memo[0] = true;
            prev[0] = new KeyValuePair<int, int>(-1, 0);

            for (int i = 0; i < inputArray.Count; ++i)
            {
                int num = inputArray[i];
                for (int s = sum; s >= num; --s)
                {
                    if (memo.ContainsKey(s - num) && memo[s - num] == true)
                    {
                        memo[s] = true;

                        if (!prev.ContainsKey(s))
                        {
                            prev[s] = new KeyValuePair<int, int>(i, num);
                        }
                    }
                }
            }
        }


        public static int getNearKey(IEnumerable<int> col, int value)
        {
            return col.Select(x => new
            {
                x,
                distance = Math.Abs(x - value)
            }).OrderBy(p => p.distance).First().x;
        }

        public static KeyValuePair<int, int> getNearKeyDict(Dictionary<int, KeyValuePair<int, int>> col, int key)
        {
            int temp = key;
            KeyValuePair<int, int> res = new KeyValuePair<int, int>();
            foreach (var el in col)
            {
                if (temp > Math.Abs(el.Key - key))
                {
                    temp = Math.Abs(el.Key - key);
                    res = el.Value;
                    tempsum = el.Key;
                }
            }
            return res;

        }


        public static KeyValuePair<int, int> getKeyValue(Dictionary<int, KeyValuePair<int, int>> col, int key)
        {
            if (col.ContainsKey(key))
            {
                tempsum = key;
                return col[key];
            }
            else
            {
                return getNearKeyDict(col, key);

            }
            // ? prev[sum].Key != -1 : prev[getNearKey(prev, sum)].Key != -1;

        }


        public static void fixsum(int sum)
        {
            getKeyValue(prev, sum);
        }

        public static IEnumerable<int> GetLastResult(int sum)
        {
            while (prev[tempsum].Key != -1)
            {
                yield return prev[tempsum].Key;
                tempsum -= prev[tempsum].Value;
            }
        }
    }




}