namespace SGF.Network.FSPLite.Client
{
    public class FSPFrameController
    {
        //buffer setting
        private int mNewestFrameId;
        private int mBuffSize = 0;
        private bool mIsInBuffing = false;
        private int mClientFrameRateMultiple = 2; //Client_FPS divides Server_FPS

        //speed up control
        private bool mEnableSpeedUp = true;
        private int mDefaultSpeed = 1;
        private bool mIsInSpeedUp = false;

        //auto buffer
        private bool mEnableAutoBuff = true;
        private int mAutoBuffCnt = 0;
        private int mAutoBuffInterval = 15;

        public void Start(FSPParam param)
        {
            SetParam(param);
        }

        public void Close()
        {
        }

        public bool IsInBuffing { get { return mIsInBuffing; } }
        public bool IsInSpeedUp { get { return mIsInSpeedUp; } }
        public int FrameBufferSize { get { return mBuffSize; } set { mBuffSize = value; } }
        public int NewestFrameId { get { return mNewestFrameId; } }

        public void SetParam(FSPParam param)
        {
            mClientFrameRateMultiple = param.clientFrameRateMultiple;
            mBuffSize = param.frameBufferSize;
            mEnableSpeedUp = param.enableSpeedUp;
            mDefaultSpeed = param.defaultSpeed;
            mEnableAutoBuff = param.enableAutoBuffer;
        }




        public void AddFrameId(int frameId)
        {
            mNewestFrameId = frameId;
        }


        public int GetFrameSpeed(int curFrameId)
        {
            int speed = 0;

            int newFrameNum = mNewestFrameId - curFrameId;

            //if is not buffering
            if (!mIsInBuffing)
            {
 
                if (newFrameNum == 0)
                {
                    //buffer in
                    mIsInBuffing = true;
                    mAutoBuffCnt = mAutoBuffInterval;
                }
                else
                {
                    //will play [defaultSpeed] frames
                    newFrameNum -= mDefaultSpeed;

                    //speed up frames
                    int speedUpFrameNum = newFrameNum - mBuffSize;
                    if (speedUpFrameNum >= mClientFrameRateMultiple)
                    {
                        //speedup enabled
                        if (mEnableSpeedUp)
                        {
                            speed = speedUpFrameNum > 100 ? 8 : 2;
                        }
                        else
                        {
                            speed = mDefaultSpeed;
                        }
                    }
                    else
                    {
                        //speedup frame number not enough
                        speed = mDefaultSpeed;

                        //auto buff when frame is too little
                        //better stuck for a while than get stuck in each frame

                        if (mEnableAutoBuff)
                        {
                            mAutoBuffCnt--;
                            if (mAutoBuffCnt <= 0)
                            {
                                mAutoBuffCnt = mAutoBuffInterval;
                                if (speedUpFrameNum < mClientFrameRateMultiple - 1)
                                {
                                    speed = 0;
                                }
                            }
                        }
                    }
                }
            }
            else
            {
                //in buffering process

                //speed up frames
                int speedUpFrameNum = newFrameNum - mBuffSize;
                if (speedUpFrameNum > 0)
                {
                    //end buffering
                    mIsInBuffing = false;
                }
            }

            mIsInSpeedUp = speed > mDefaultSpeed;
            return speed;
        }
    }
}
