using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Reflection;
using System.Runtime.InteropServices;

using NUnit.Framework;

namespace Com.Luxiar.Mikako.Util
{
    /// <summary>
    /// コードの実行時間を計測するクラス。
    /// 経過時間をms単位でコンソールに出力します。
    /// 
    /// </summary>
    /// <example>
    /// 使い方１： メソッド内の処理の一部分を計測する場合、スコープを区切りません。
    ///   途中でreturnした場合などStopメソッドを呼ばれない場合、計測できません。
    /// 
    ///   PerformanceTimer3.Start();
    ///     ここに実行時間を計測したい処理
    ///   PerformanceTimer3.Finish();
    /// 
    /// 使い方２：
    ///   メソッド内の処理を全て計測する場合など、スコープ区切りを気にしない場合
    ///   途中でreturnしても例外が上がっても計測可能です。
    ///   
    ///   using(new PerformanceTimer3()){
    ///     ここに実行時間を計測したい処理
    ///   }
    /// </example>
    public class PerformanceTimer3 : IDisposable
    {
        #region インスタンスメソッド
        private readonly WhiteRabbit _rabbit = null;

        /// <summary>
        /// Disposeメソッド呼び出しまでの経過時間を計測します。
        /// </summary>
        public PerformanceTimer3()
        {
#if DEBUG
            WhiteRabbit rabbit = new WhiteRabbit(Prefix);
            _rabbit = rabbit;
#endif
        }

        public void Dispose()
        {
            if(_rabbit != null )
                _rabbit.Catch(1);
        }
        #endregion

        #region クラスメソッド
        /// <summary>
        /// 計測開始
        /// </summary>
        [Conditional("DEBUG")]
        public static void Start()
        {
            WhiteRabbit rabbit = new WhiteRabbit(Prefix);
            FollowRabbit(rabbit, InvokerMethodName);
        }

        /// <summary>
        /// 計測開始
        /// </summary>
        /// <param name="message">追加メッセージ</param>
        [Conditional("DEBUG")]
        public static void Start(string message)
        {
            WhiteRabbit rabbit = new WhiteRabbit(Prefix, message);
            FollowRabbit(rabbit, InvokerMethodName);
        }

        /// <summary>
        /// 計測終了
        /// </summary>
        /// <exception cref="System.InvalidOperationException">Startせずに呼び出した場合</exception>
        [Conditional("DEBUG")]
        public static void Stop()
        {
            try
            {
                Stack<WhiteRabbit> s = _wonderLand[InvokerMethodName];
                s.Pop().Catch(s.Count);
            }
            catch (KeyNotFoundException e)
            {
                throw new InvalidOperationException("同一メソッド内でStartしてからStopしてください", e);
            }
        }

        private static readonly Dictionary<String, Stack<WhiteRabbit>> _wonderLand = new Dictionary<string, Stack<WhiteRabbit>>();
        private static void FollowRabbit(WhiteRabbit rabbit, string stackName)
        {
            if (!_wonderLand.ContainsKey(stackName))
            {
                _wonderLand[stackName] = new Stack<WhiteRabbit>();
            }
            _wonderLand[stackName].Push(rabbit);
        }

        /// <summary>
        /// 呼び出し位置に依存するので注意
        /// </summary>
        private static string InvokerMethodName
        {
            get
            {
                MethodBase mb = new StackTrace(true).GetFrame(2).GetMethod();
                return mb.DeclaringType + "." + mb.Name;
            }
        }
        #endregion

        /// <summary>
        /// 呼び出し位置に依存するので注意
        /// 出力ウィンドウでダブルクリックして飛べる用にファイル名、行数、列数を先頭に表示します。
        /// </summary>
        private static string Prefix
        {
            get
            {
                StackFrame sf = new StackTrace(true).GetFrame(2);
                MethodBase mb = sf.GetMethod();
                string msg = String.Format(
                    "{0}({1},{2}): 計測メソッド名: {3}.{4}",
                    sf.GetFileName(),
                    sf.GetFileLineNumber(),
                    sf.GetFileColumnNumber(),
                    mb.DeclaringType,
                    mb.Name
                    );
                return msg;
            }
        }

        /// <summary>
        /// 時計を持って走る兎クラスです。
        /// </summary>
        private class WhiteRabbit
        {
            [DllImport("kernel32.dll")]
            private extern static short QueryPerformanceCounter(ref long x);

            [DllImport("kernel32.dll")]
            private extern static short QueryPerformanceFrequency(ref long x);

            private readonly string _firstName = "";
            private readonly string _lastName = "";
            private readonly long _cntAtStart = 0;

            public WhiteRabbit(string firstName)
                : this(firstName, "") { }

            public WhiteRabbit(string firstName, string lastName)
            {
                _firstName = firstName;
                _lastName = lastName;
                QueryPerformanceCounter(ref _cntAtStart);
            }

            public void Catch(int idForRabbit)
            {
                string aaa = _lastName == ""
                    ? String.Format("{0} {1} , 経過時間: {2:F}ms", _firstName, idForRabbit, ElapsedMilliSec)
                    : String.Format("{0} {1} \"{2}\", 経過時間: {3:F}ms", _firstName, idForRabbit, _lastName, ElapsedMilliSec);
                Trace.WriteLine(aaa);
            }

            private double ElapsedMilliSec
            {
                get
                {
                    long cntAtEnd = 0;
                    QueryPerformanceCounter(ref cntAtEnd);

                    long freqOfCounter = 0;
                    QueryPerformanceFrequency(ref freqOfCounter);

                    return (double)(cntAtEnd - _cntAtStart) / freqOfCounter * 1000;
                }
            }
        }

        #region テスト
        [TestFixture]
        public class Test
        {
            [Test]
            public void インスタンスメソッドの標準的な使い方()
            {
                using (new PerformanceTimer3())
                    return;
            }

            [Test]
            public void クラスメソッドの標準的な使い方()
            {
                PerformanceTimer3.Start();
                PerformanceTimer3.Stop();
                PerformanceTimer3.Start();
                PerformanceTimer3.Stop();
                PerformanceTimer3.Start("追加メッセージ");
                PerformanceTimer3.Stop();
            }

            [Test]
            public void 呼び出しの入れ子()
            {
                PerformanceTimer3.Start("1");
                PerformanceTimer3.Start("2");
                PerformanceTimer3.Stop();
                PerformanceTimer3.Stop();
            }

            [Test]
            public void メソッド単位でタイマーを管理します()
            {
                PerformanceTimer3.Start();
                InnerFunction();
                PerformanceTimer3.Stop();
            }

            private void InnerFunction()
            {
                PerformanceTimer3.Start();
            }

            [Test]
            public void 無名関数内で使うと()
            {
                Predicate<string> p = delegate(String s)
                {
                    PerformanceTimer3.Start(s);
                    PerformanceTimer3.Stop();
                    return false;
                };

                PerformanceTimer3.Start();
                p("AAA");
                PerformanceTimer3.Stop();
            }

            [Test]
            [ExpectedException(ExceptionType = typeof(InvalidOperationException))]
            public void StartせずにStopすると()
            {
                PerformanceTimer3.Stop();
            }
        }
        #endregion
    }
}
