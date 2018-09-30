using ProtoBuf;

namespace Snaker.GameCore.Data
{
    /// <summary>
    /// data for snakes in a game, it is different from configuration data of snakes
    /// if we implement growing system for snake, we need to differentiate configuration data and data of enforced snakes
    /// </summary>
    [ProtoContract]
    public class SnakeData
    {
        /// <summary>
        /// id for snakes, refers to what kind of snake it is. can be used to locate the resources and configuration of snakes
        /// why do use the configuration data of snake directely?
        /// coz different players would use the same kind of snaker, after enforement, the parameters of snakers would be different
        /// like the original length of snakes.
        /// </summary>
        [ProtoMember(1)]
        public int id;

        /// <summary>
        /// name of snake
        /// </summary>
        [ProtoMember(2)]
        public string name = "";


        /// <summary>
        /// the size of snake muscle unit
        /// it is a configuration value used to determine the size of resource
        /// </summary>
        [ProtoMember(3)]
        public int size = 32;

        /// <summary>
        /// how many core units are contain in a muscle unit
		/// based on observation, 5 is the best option regarding the smoothness of animation
        /// </summary>
		[ProtoMember(4)]
        public int keyStep = 5;


        /// <summary>
        /// the number of core units
        /// the default value for different snakes is different. even the same snake for different players has different
        /// core units.
        /// as the game progresses, the value would increse.
        /// </summary>
        [ProtoMember(5)]
        public int length = 50;



        /// <summary>
        /// as the snake grow larger, the view scale should increase as well
        /// </summary>
        [ProtoMember(6)]
        public float viewScale = 1;

        /// <summary>
        /// the visibility of the body of snake. some effect requires to hide the body of snakes
        /// </summary>
        [ProtoMember(7)]
        public bool bodyVisible = true;
    }
}
