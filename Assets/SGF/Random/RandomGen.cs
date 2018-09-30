namespace SGF.Random
{
    public class RandomGen
    {
        #region Default Instance
        public static RandomGen Default = new RandomGen();
        #endregion

        #region paras for linear congruence

        private const int PrimeA = 214013;
        private const int PrimeB = 2531011;

        #endregion

        //normalization
        private const float Mask15Bit_1 = 1.0f / 0x7fff;
        private const int Mask15Bit = 0x7fff;

        private int m_Value = 0;

        public int Seed
        {
            set { m_Value = value; }
            get { return m_Value; }
        }

        /// <summary>
        /// generate random number from 0-1
        /// </summary>
        /// <returns></returns>
        public float Rnd()
        {
            float val = ((((m_Value = m_Value * PrimeA + PrimeB) >> 16) & Mask15Bit) - 1) * Mask15Bit_1;
            return (val > 0.99999f ? 0.99999f : val);
        }

        public float Range(float min, float max)
        {
            return min + Rnd() * (max - min);
        }

        public int Range(int min, int max)
        {
            return (int)(min + Rnd() * (max - min));
        }


        public float Range(float max)
        {
            return Range(0, max);
        }

        public int Range(int max)
        {
            return Range(0, max);
        }



    }
}
