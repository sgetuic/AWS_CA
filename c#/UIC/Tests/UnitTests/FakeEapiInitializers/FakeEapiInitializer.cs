using System;
using System.Runtime.InteropServices;
using UIC.EDM.EApi.Shared;

namespace UnitTests.FakeEapiInitializers
{
    internal class FakeEapiInitializer : UIC.Framework.Interfaces.Eapi.IEapiInitializer
    {
        private readonly EApiStatusCodes _eApiStatusCodes;

        private static bool _initDone = false;
        private static bool _isDispose = false;
        private static bool _initFailed = false;

        public bool GetInitDone() => _initDone;
        public bool GetDispose() => _isDispose;
        public bool GetInitFailed() => _initFailed;

        [DllImport("Eapi_1.dll")]
        public static extern UInt32 EApiLibInitialize();

        [DllImport("Eapi_1.dll")]
        public static extern UInt32 EApiLibUnInitialize();

        public FakeEapiInitializer()
        {
            _eApiStatusCodes = new EApiStatusCodes();
        }


        public void Init()
        {
            if (_initFailed)
            {
                throw new Exception("Eapi Init failed");
            }

            if (_initDone) return;
            _initDone = true;
            _isDispose = false;

            try
            {
                // because of test purpesse
                //var result = EApiLibInitialize();
                //if (!_eApiStatusCodes.IsSuccess(result))
                //{
                //    throw new Exception("Eapi Init failed: " + _eApiStatusCodes.GetStatusStringFrom(result));
                //}
            }
            catch (Exception)
            {
                _initFailed = true;
                throw;
            }
        }

        public void Dispose()
        {

            if (_isDispose) return;
            _isDispose = true;
            _initDone = false;

            //var result = EApiLibUnInitialize();
            //if (!_eApiStatusCodes.IsSuccess(result))
            //{
            //    throw new Exception("Eapi Uninitialize failed: " + _eApiStatusCodes.GetStatusStringFrom(result));
            //}
        }
    }
}
