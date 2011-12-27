using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Reflection;
using System.Runtime.InteropServices;

using NUnit.Framework;

namespace Com.Luxiar.Mikako.Util
{
    /// <summary>
    /// �R�[�h�̎��s���Ԃ��v������N���X�B
    /// �o�ߎ��Ԃ�ms�P�ʂŃR���\�[���ɏo�͂��܂��B
    /// 
    /// </summary>
    /// <example>
    /// �g�����P�F ���\�b�h���̏����̈ꕔ�����v������ꍇ�A�X�R�[�v����؂�܂���B
    ///   �r����return�����ꍇ�Ȃ�Stop���\�b�h���Ă΂�Ȃ��ꍇ�A�v���ł��܂���B
    /// 
    ///   PerformanceTimer3.Start();
    ///     �����Ɏ��s���Ԃ��v������������
    ///   PerformanceTimer3.Finish();
    /// 
    /// �g�����Q�F
    ///   ���\�b�h���̏�����S�Čv������ꍇ�ȂǁA�X�R�[�v��؂���C�ɂ��Ȃ��ꍇ
    ///   �r����return���Ă���O���オ���Ă��v���\�ł��B
    ///   
    ///   using(new PerformanceTimer3()){
    ///     �����Ɏ��s���Ԃ��v������������
    ///   }
    /// </example>
    public class PerformanceTimer3 : IDisposable
    {
        #region �C���X�^���X���\�b�h
        private readonly WhiteRabbit _rabbit = null;

        /// <summary>
        /// Dispose���\�b�h�Ăяo���܂ł̌o�ߎ��Ԃ��v�����܂��B
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

        #region �N���X���\�b�h
        /// <summary>
        /// �v���J�n
        /// </summary>
        [Conditional("DEBUG")]
        public static void Start()
        {
            WhiteRabbit rabbit = new WhiteRabbit(Prefix);
            FollowRabbit(rabbit, InvokerMethodName);
        }

        /// <summary>
        /// �v���J�n
        /// </summary>
        /// <param name="message">�ǉ����b�Z�[�W</param>
        [Conditional("DEBUG")]
        public static void Start(string message)
        {
            WhiteRabbit rabbit = new WhiteRabbit(Prefix, message);
            FollowRabbit(rabbit, InvokerMethodName);
        }

        /// <summary>
        /// �v���I��
        /// </summary>
        /// <exception cref="System.InvalidOperationException">Start�����ɌĂяo�����ꍇ</exception>
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
                throw new InvalidOperationException("���ꃁ�\�b�h����Start���Ă���Stop���Ă�������", e);
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
        /// �Ăяo���ʒu�Ɉˑ�����̂Œ���
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
        /// �Ăяo���ʒu�Ɉˑ�����̂Œ���
        /// �o�̓E�B���h�E�Ń_�u���N���b�N���Ĕ�ׂ�p�Ƀt�@�C�����A�s���A�񐔂�擪�ɕ\�����܂��B
        /// </summary>
        private static string Prefix
        {
            get
            {
                StackFrame sf = new StackTrace(true).GetFrame(2);
                MethodBase mb = sf.GetMethod();
                string msg = String.Format(
                    "{0}({1},{2}): �v�����\�b�h��: {3}.{4}",
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
        /// ���v�������đ���e�N���X�ł��B
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
                    ? String.Format("{0} {1} , �o�ߎ���: {2:F}ms", _firstName, idForRabbit, ElapsedMilliSec)
                    : String.Format("{0} {1} \"{2}\", �o�ߎ���: {3:F}ms", _firstName, idForRabbit, _lastName, ElapsedMilliSec);
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

        #region �e�X�g
        [TestFixture]
        public class Test
        {
            [Test]
            public void �C���X�^���X���\�b�h�̕W���I�Ȏg����()
            {
                using (new PerformanceTimer3())
                    return;
            }

            [Test]
            public void �N���X���\�b�h�̕W���I�Ȏg����()
            {
                PerformanceTimer3.Start();
                PerformanceTimer3.Stop();
                PerformanceTimer3.Start();
                PerformanceTimer3.Stop();
                PerformanceTimer3.Start("�ǉ����b�Z�[�W");
                PerformanceTimer3.Stop();
            }

            [Test]
            public void �Ăяo���̓���q()
            {
                PerformanceTimer3.Start("1");
                PerformanceTimer3.Start("2");
                PerformanceTimer3.Stop();
                PerformanceTimer3.Stop();
            }

            [Test]
            public void ���\�b�h�P�ʂŃ^�C�}�[���Ǘ����܂�()
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
            public void �����֐����Ŏg����()
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
            public void Start������Stop�����()
            {
                PerformanceTimer3.Stop();
            }
        }
        #endregion
    }
}
