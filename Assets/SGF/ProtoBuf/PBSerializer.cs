////////////////////////////////////////////////////////////////////
//                            _ooOoo_                             //
//                           o8888888o                            //
//                           88" . "88                            //
//                           (| ^_^ |)                            //
//                           O\  =  /O                            //
//                        ____/`---'\____                         //

//                             `=---='                            //
//        ^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^      //
//          NO BUG PLEASE~~                  //
////////////////////////////////////////////////////////////////////

using ProtoBuf;
using ProtoBuf.Meta;
using System.IO;

namespace SGF.ProtoBuf
{
    /// <summary>
    /// ProtoBuf Utils
    /// </summary>
    public class PBSerializer
    {

        public static T Clone<T>(T data)
        {
            byte[] buffer = NSerialize<T>(data);
            return NDeserialize<T>(buffer);
        }


        /// <summary>
        /// Serilize PB data
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="t"></param>
        /// <returns></returns>
        public static byte[] NSerialize<T>(T t)
        {
            byte[] buffer = null;

            using (MemoryStream m = new MemoryStream())
            {
                Serializer.Serialize<T>(m, t);

                m.Position = 0;
                int length = (int)m.Length;
                buffer = new byte[length];
                m.Read(buffer, 0, length);
            }

            return buffer;
        }





        public static byte[] NSerialize(object t)
        {
            byte[] buffer = null;

            using (MemoryStream m = new MemoryStream())
            {
                if (t != null)
                {
                    RuntimeTypeModel.Default.Serialize(m, t);
                }

                m.Position = 0;
                int length = (int)m.Length;
                buffer = new byte[length];
                m.Read(buffer, 0, length);
            }

            return buffer;
        }


        public static int NSerialize(object t, byte[] buffer)
        {
            using (MemoryStream m = new MemoryStream())
            {
                if (t != null)
                {
                    RuntimeTypeModel.Default.Serialize(m, t);
                }

                m.Position = 0;
                int length = (int)m.Length;
                m.Read(buffer, 0, length);
                return length;
            }
            return 0;
        }



        /// <summary>
        /// Deserialize data
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="buffer"></param>
        /// <returns></returns>
        public static T NDeserialize<T>(byte[] buffer)
        {
            T t = default(T);
            using (MemoryStream m = new MemoryStream(buffer))
            {
                t = Serializer.Deserialize<T>(m);
            }
            return t;
        }

        public static object NDeserialize(byte[] buffer, System.Type type)
        {
            object t = null;
            using (MemoryStream m = new MemoryStream(buffer))
            {
                t = RuntimeTypeModel.Default.Deserialize(m, null, type);
            }
            return t;
        }

        public static object NDeserialize(byte[] buffer, int len, System.Type type)
        {
            object t = null;
            using (MemoryStream m = new MemoryStream(buffer))
            {
                t = RuntimeTypeModel.Default.Deserialize(m, null, type, len);
            }
            return t;
        }


        public static T NDeserialize<T>(Stream stream)
        {
            T t = default(T);
            t = Serializer.Deserialize<T>(stream);
            return t;
        }
    }

}
