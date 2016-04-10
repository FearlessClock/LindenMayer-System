using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using OpenTK;
using OpenTK.Graphics.OpenGL;
using OpenTK.Input;
using System.Drawing;
using System.IO;
using System.Xml;

namespace LindenMayer_System
{
    struct Vertex
    {
        public Vector2 position;
        public Vector2 texCoord;
        public Vector4 color;

        public Color Color
        {
            get
            {
                return Color.FromArgb((int)(255 * color.W), (int)(255 * color.X), (int)(255 * color.Y), (int)(255 * color.Z));
            }
            set
            {
                this.color = new Vector4(value.R / 255f, value.G / 255f, value.B / 255f, value.A / 255f);
            }

        }
        static public int SizeInBytes
        {
            get { return Vector2.SizeInBytes * 2 + Vector4.SizeInBytes; }
        }

        public Vertex(Vector2 position, Vector2 texCoord)
        {
            this.position = position;
            this.texCoord = texCoord;
            this.color = new Vector4(1, 1, 1, 1);
        }


    }
    class Game
    {
        public GameWindow window;
        Texture2D texture;

        //Start of the vertex buffer
        Vertex[] vertBuffer;
        int VBO; //Vertex buffer object

        uint[] indexBuffer;
        int IBO;    //Index buffer object

        int nmbr = 1;
        TurtleGraphics[] superTurtle;
        TurtleGraphics mountainTurtle;
        string[] axiom;
        string axiom2;

        LindenmayerSystem[] lSystem;
        LindenmayerSystem mountainSystem;
        int counter = 0;

        //Camera position
        Vector2 cameraPos = new Vector2(400, 300);
        Vector3 fractalRot = new Vector3(0, 0, 0);
        Vector2 fractalScale = new Vector2(1, 1);
        int step = 5;

        public Game(GameWindow windowInput)
        {

            superTurtle = new TurtleGraphics[nmbr];
            axiom = new string[nmbr];
            lSystem = new LindenmayerSystem[nmbr];
            window = windowInput;

            window.Load += Window_Load;
            window.RenderFrame += Window_RenderFrame;
            window.UpdateFrame += Window_UpdateFrame;
            window.Closing += Window_Closing;
        }


        private void Window_Load(object sender, EventArgs e)
        {
            texture = ContentPipe.LoadTexture("explo.bmp");

            vertBuffer = new Vertex[0]
            {
            };


            VBO = GL.GenBuffer();
            GL.BindBuffer(BufferTarget.ArrayBuffer, VBO);
            GL.BufferData<Vertex>(BufferTarget.ArrayBuffer, (IntPtr)(Vertex.SizeInBytes * vertBuffer.Length), vertBuffer, BufferUsageHint.StaticDraw);
            GL.BindBuffer(BufferTarget.ArrayBuffer, 0);

            indexBuffer = new uint[0]
            {
            };

            IBO = GL.GenBuffer();
            GL.BindBuffer(BufferTarget.ElementArrayBuffer, IBO);
            GL.BufferData(BufferTarget.ElementArrayBuffer, (IntPtr)(sizeof(uint) * (indexBuffer.Length)), indexBuffer, BufferUsageHint.StaticDraw);
            GL.BindBuffer(BufferTarget.ElementArrayBuffer, 0);

            //Content/LindemSystemRules/settingsTree.xml
            string[] fileName = new string[1] { "Content/LindemSystemRules/Walker/settingsWalker2.xml" };///Console.ReadLine();
            for (int i = 0; i < superTurtle.Length; i++)
            {
                superTurtle[i] = new TurtleGraphics(fileName[i]);//new Vector2(0, 0), true, 90);
                lSystem[i] = new LindenmayerSystem(fileName[i]);
                XmlDocument settings = new XmlDocument();
                settings.Load(fileName[i]);
                XmlNode node = settings.SelectSingleNode("//LindenMayerSystem/axiom");
                //StreamReader sr = new StreamReader("Content/Walker/settingsWalker.");
                axiom[i] = node.Attributes["axiom"].Value;// sr.ReadToEnd();
            }
            
            
            
            /*
            fileName = "Content/LindemSystemRules/settingsMountain.xml";///Console.ReadLine();
            mountainTurtle = new TurtleGraphics(fileName);//new Vector2(0, 0), true, 90);
            mountainSystem = new LindenmayerSystem(fileName);
            settings = new XmlDocument();
            settings.Load(fileName);
            node = settings.SelectSingleNode("//LindenMayerSystem/axiom");
            axiom2 = node.Attributes["axiom"].Value;*/
        }
        private void Window_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {

        }
        bool end = false;
        private void Window_UpdateFrame(object sender, FrameEventArgs e)
        {
            KeyboardState key = Keyboard.GetState();
            if (!end)
            //if (Console.ReadLine() != "s")
            {
                List<Vertex> vert = new List<Vertex>();
                List<uint> index = new List<uint>();
                uint counter = 1;
                for (int i = 0; i < axiom.Length; i++)
                {
                    axiom[i] = lSystem[i].Calculate(axiom[i]);
                    //axiom2 = mountainSystem.Calculate(axiom2);
                    //Console.WriteLine(axiom2);
                    int size = vert.Count;
                    vert.AddRange(superTurtle[i].Draw(axiom[i]));

                    uint[] indexMid = superTurtle[i].GetIndexBuffer();
                    for (int j = 0; j < indexMid.Length; j++)
                    {
                        indexMid[j] += (uint)size;
                    }
                    index.AddRange(indexMid);
                    counter++;
                    
                }

                vertBuffer = vert.ToArray<Vertex>();
                indexBuffer = index.ToArray<uint>();
                GL.BindBuffer(BufferTarget.ArrayBuffer, VBO);
                GL.BufferData<Vertex>(BufferTarget.ArrayBuffer, (IntPtr)(Vertex.SizeInBytes * vertBuffer.Length), vertBuffer, BufferUsageHint.StaticDraw);
                GL.BindBuffer(BufferTarget.ElementArrayBuffer, IBO);
                GL.BufferData(BufferTarget.ElementArrayBuffer, (IntPtr)(sizeof(uint) * (indexBuffer.Length)), indexBuffer, BufferUsageHint.StaticDraw);
                GL.BindBuffer(BufferTarget.ElementArrayBuffer, 0);

                //Console.WriteLine(axiom);
                counter++;
                if (counter >= 1)
                    end = true;
                if (key.IsKeyDown(Key.C)) 
                    end = true;
            }
            int booster = 1;
            if (key.IsKeyDown(Key.Q))
                window.Close();
            if (key.IsKeyDown(Key.ShiftRight))
                booster = 2;
            if (key.IsKeyUp(Key.ShiftRight))
                booster = 1;
            if (key.IsKeyDown(Key.Keypad5))
                cameraPos.Y -= step*booster;
            else if (key.IsKeyDown(Key.Keypad8))
                cameraPos.Y += step * booster;
            if (key.IsKeyDown(Key.Keypad4))
                cameraPos.X -= step * booster;
            else if (key.IsKeyDown(Key.Keypad6))
                cameraPos.X += step * booster;

            if (key.IsKeyDown(Key.Keypad7))
                fractalRot.Z += MathHelper.DegreesToRadians(1);
            else if (key.IsKeyDown(Key.Keypad9))
                fractalRot.Z -= MathHelper.DegreesToRadians(1);

            if (key.IsKeyDown(Key.Keypad1))
                fractalScale.X += 0.04F;
            else if (key.IsKeyDown(Key.Keypad3))
                fractalScale.X -= 0.04F;

            if(key.IsKeyDown(Key.N))
            {
                end = false;
            }
        }

        private void Window_RenderFrame(object sender, FrameEventArgs e)
        {
            //Clear screen color
            GL.ClearColor(Color.Black);
            GL.Clear(ClearBufferMask.ColorBufferBit);

            //Enable color blending, which allows transparency
            GL.Enable(EnableCap.Blend);
            GL.Enable(EnableCap.Texture2D);
            //Blending everything for transparency
            GL.BlendFunc(BlendingFactorSrc.SrcAlpha, BlendingFactorDest.OneMinusSrcAlpha);

            //Create the projection matrix for the scene
            Matrix4 proj = Matrix4.CreateOrthographicOffCenter(0, window.Width, 0, window.Height, 0, 1);
            GL.MatrixMode(MatrixMode.Projection);
            GL.LoadMatrix(ref proj);

            //Bind the texture that will be used
            GL.BindTexture(TextureTarget.Texture2D, texture.ID);

            //Enable all the different arrays
            GL.EnableClientState(ArrayCap.ColorArray);
            GL.EnableClientState(ArrayCap.VertexArray);
            GL.EnableClientState(ArrayCap.TextureCoordArray);

            //Load the vert and index buffers
            GL.BindBuffer(BufferTarget.ArrayBuffer, VBO);
            GL.VertexPointer(2, VertexPointerType.Float, Vertex.SizeInBytes, (IntPtr)0);
            GL.TexCoordPointer(2, TexCoordPointerType.Float, Vertex.SizeInBytes, (IntPtr)(Vector2.SizeInBytes));
            GL.ColorPointer(4, ColorPointerType.Float, Vertex.SizeInBytes, (IntPtr)(Vector2.SizeInBytes * 2));
            GL.BindBuffer(BufferTarget.ElementArrayBuffer, IBO);

            //Create a scale matrux
            Matrix4 mat = Matrix4.Identity;

            GL.MatrixMode(MatrixMode.Modelview);    //Load the modelview matrix, last in the chain of view matrices
            GL.LoadMatrix(ref mat);

            

            mat = Matrix4.CreateTranslation(cameraPos.X, cameraPos.Y, 0);  //Create a translation matrix
            GL.MultMatrix(ref mat);                 //Load the translation matrix into the modelView matrix

            mat = Matrix4.CreateRotationX(fractalRot.X);
            GL.MultMatrix(ref mat);
            mat = Matrix4.CreateRotationY(fractalRot.Y);
            GL.MultMatrix(ref mat);
            mat = Matrix4.CreateRotationZ(fractalRot.Z);
            GL.MultMatrix(ref mat);

            mat = Matrix4.CreateScale(fractalScale.X, fractalScale.X, 0);
            GL.MultMatrix(ref mat);                     //Multiply the scale matrix with the modelview matrix
            GL.PushMatrix();
            GL.DrawElements(PrimitiveType.Lines, indexBuffer.Length, DrawElementsType.UnsignedInt, 0);


            //Flush everything 
            GL.Flush();
            //Write the new buffer to the screen
            window.SwapBuffers();
        }
    }
}
