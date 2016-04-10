using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using OpenTK;
using OpenTK.Graphics.OpenGL;
using System.Drawing;
using System.IO;
using System.Xml;

namespace LindenMayer_System
{
    enum Functions { Forward, Left, Right, Pop, Push };
    struct Command
    {
        public char variable;  //LindenMayerSystem variable from the string
        public Functions function;     //Function the variables is assoiciated with
        public int value;              //Value sent to the function (Angle, distance)
        public Color color;            //Color of the forward movement
        public bool penUp;             //If the pen is up or down

        public Command(char var, string func)
        {
            variable = var;
            switch(func)
            {
                case "Forward":
                    function = Functions.Forward;
                    break;
                case "Left":
                    function = Functions.Left;
                    break;
                case "Right":
                    function = Functions.Right;
                    break;
                case "Push":
                    function = Functions.Push;
                    break;
                case "Pop":
                    function = Functions.Pop;
                    break;
                default:
                    function = Functions.Forward;
                    break;
            }
            value = -1;
            color = Color.Black;
            penUp = false;
        }
        public Command(char var, string func, int val) : this(var, func)
        {
            value = val;
        }
        public Command(char var, string func, int val, Color col, bool pen): this(var, func, val)
        {
            color = col;
            penUp = pen;
        }
    }
    class TurtleGraphics
    {
        List<Command> commands = new List<Command>();

        XmlDocument settings;
        public Vector2 position;           //Current position of the turtle
        public Vector2 startingPosition;

        bool isPenDown;             //Is the pen down to draw
        float direction;            //In degrees, The direction the turtle is facing in   
        float startingDirection;

        List<Vertex> imageDrawn;   //A list containing all the xy points for the lines
        List<uint> index;

        string word;
        public string Word
        {
            get { return word; }
            set { word = value; }
        }

        Stack<Vector2> vecStack = new Stack<Vector2>();
        Stack<bool> penStack = new Stack<bool>();
        Stack<float> dirStack = new Stack<float>();

        //Variables for the index buffer
        uint last;
        uint next;
        List<uint> indexBuffer;
        Stack<uint> stackUint;

        Random rand = new Random();
        //Color array 
        Color[] colorSelection = new Color[13] { Color.Yellow, Color.LightYellow, Color.Orange, Color.OrangeRed, Color.Red, Color.PaleVioletRed, Color.Violet, Color.BlueViolet, Color.Blue, Color.LightBlue, Color.LightGreen, Color.Green, Color.GreenYellow };
        Color[] branchColorSelection = new Color[2] { Color.Brown, Color.SaddleBrown };
        Color[] leafColorSelection = new Color[5] { Color.Green, Color.GreenYellow, Color.DarkSeaGreen, Color.LawnGreen, Color.ForestGreen };
        
        //List of Variables, assoiciated functions and variables
        
        /// <summary>
        /// TurtleGraphics object used to convert a string into an image.
        /// </summary>
        /// <param name="pos">Initail position</param>
        /// <param name="penState">Pen up or down? Down is true</param>
        /// <param name="dir">Direction the turtle is looking in, in degrees</param>
        public TurtleGraphics(Vector2 pos, bool penState, float dir)
        {
            imageDrawn = new List<Vertex>();
            index = new List<uint>();
            position = pos;
            isPenDown = penState;
            direction = dir;

            last = 0;
            next = 1;
            indexBuffer = new List<uint>();
            stackUint = new Stack<uint>();

        }

        public TurtleGraphics(string fileName)
        {
            commands = new List<Command>();
            settings = new XmlDocument();
            settings.Load(fileName);
            //Get all the starting information
            XmlNodeList xmlNodes = settings.SelectNodes("//LindenMayerSystem/TurtleGraphics/StartingValues");
            foreach (XmlNode starting in xmlNodes)
            {
                startingPosition.X = (float)Convert.ToDouble(starting.Attributes["X"].Value);
                startingPosition.Y = (float)Convert.ToDouble(starting.Attributes["Y"].Value);
                startingDirection = (float)Convert.ToDouble(starting.Attributes["Direction"].Value);
            }
            //Get all the variables stuff
            xmlNodes = settings.SelectNodes("//LindenMayerSystem/TurtleGraphics/VariableSignifications/variable");
            foreach (XmlNode vars in xmlNodes)
            {
                commands.Add(new Command(vars.Attributes["var"].Value.ToCharArray()[0],
                                          vars.Attributes["function"].Value));
                Command com = commands[commands.Count - 1];
                if (com.function == Functions.Forward ||
                    com.function == Functions.Left ||
                    com.function == Functions.Right)
                {
                    com.value = Convert.ToInt32(vars.Attributes["value"].Value);
                    if(com.function == Functions.Forward)
                    {
                        com.color = Color.FromName(vars.Attributes["color"].Value);
                        com.penUp = vars.Attributes["penDown"].Value.Equals("false") ? false : true;
                    }
                }
                commands[commands.Count - 1] = com;
            }

            imageDrawn = new List<Vertex>();
            index = new List<uint>();

            last = 0;
            next = 1;
            indexBuffer = new List<uint>();
            stackUint = new Stack<uint>();
        }

        /// <summary>
        /// Draw the string recieved from the lSystem
        /// </summary>
        /// <param name="stringToConvert"></param>
        /// <returns></returns>
        public Vertex[] Draw(string stringToConvert)
        {
            //Set everything to zero
            imageDrawn.Clear();
            position = startingPosition;
            direction = startingDirection;
            stackUint.Clear();
            indexBuffer.Clear();
            last = 0;
            next = 1;

            word = stringToConvert;
            imageDrawn.Add(new Vertex(position, new Vector2(0,0)));
            foreach (char c in word)
            {
                foreach (Command com in commands)
                {
                    if (com.variable.Equals(c))
                    {
                        switch(com.function)
                        {
                            case Functions.Forward:
                                GoForward(com.value, com.color, com.penUp);
                                break;
                            case Functions.Left:
                                TurnLeft(com.value);
                                break;
                            case Functions.Right:
                                TurnRight(com.value);
                                break;
                            case Functions.Push:
                                PushEverything();
                                break;
                            case Functions.Pop:
                                PopEverything();
                                break;
                        }
                        break;
                    }
                }
            }
            #region old Command conversion code
            /*foreach(char c in word)
            {
                switch (c)
                {
                    case 'F':
                        GoForward(2, branchColorSelection[rand.Next(0, branchColorSelection.Length)]) ;// colorSelection[rand.Next(0, colorSelection.Length)]);
                        break;
                    case 'W':
                        GoForward(2, leafColorSelection[rand.Next(0, leafColorSelection.Length)]);
                        break;
                    case 'T':
                        GoForward(4, Color.Red);
                        break;
                    case 'M':
                        GoForward(100, Color.SaddleBrown, false);
                        break;
                    case '-':
                        TurnLeft(90);
                        break;
                    case '+':
                        TurnRight(90);
                        break;
                    case 'L':
                        TurnLeft(45);
                        break;
                    case 'R':
                        TurnRight(45);
                        break;
                    case 'G':
                        TurnLeft(5);
                        break;
                    case 'D':
                        TurnRight(15);
                        break;
                    case '[':
                        PushEverything();
                        break;
                    case ']':
                        PopEverything();
                        break;
                    default:
                        break;
                }
            }*/
            #endregion

            Vertex[] vertBuf = new Vertex[imageDrawn.Count];
            int i = 0;
            foreach(Vertex vex in imageDrawn)
            {
                vertBuf[i] = vex;
                vertBuf[i].texCoord = new Vector2(i % 2, i % 2); 
                i++;
            }
            return vertBuf;
        }

        private void PopEverything()
        {
            position = vecStack.Pop();
            direction = dirStack.Pop();
            isPenDown = penStack.Pop();

            last = stackUint.Pop();
        }

        private void PushEverything()
        {
            vecStack.Push(position);
            dirStack.Push(direction);
            penStack.Push(isPenDown);

            stackUint.Push(last);
        }

        private void TurnRight(int v)
        {
            direction -= v;
        }

        private void TurnLeft(int v)
        {
            direction += v;
        }

        private void GoForward(float distance, Color c, bool pen = true)
        {
            if (pen)
            {
                indexBuffer.Add(last);
                position.X += (float)Math.Cos(MathHelper.DegreesToRadians(direction)) * distance;
                position.Y += (float)Math.Sin(MathHelper.DegreesToRadians(direction)) * distance;
                imageDrawn.Add(new Vertex(position, new Vector2(1, 1)) { Color = c });
                indexBuffer.Add(next);
                last = next;
                next++;
            }
            else
            {
                position.X += (float)Math.Cos(MathHelper.DegreesToRadians(direction)) * distance;
                position.Y += (float)Math.Sin(MathHelper.DegreesToRadians(direction)) * distance;
                imageDrawn.Add(new Vertex(position, new Vector2(1, 1)) { Color = c });
                last = next;
                next++;
            }
        }
        

        public uint[] GetIndexBuffer()
        {
            return indexBuffer.ToArray<uint>();
        }
    }
}
