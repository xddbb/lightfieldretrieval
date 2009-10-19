#region GPL EULA
// Copyright (c) 2009, Bojan Endrovski, http://furiouspixels.blogspot.com/
//
// This program is free software; you can redistribute it and/or modify
// it under the terms of the GNU Lesser General Public License as published 
// by the Free Software Foundation; either version 2 of the License, or
// (at your option) any later version.
//
// This program is distributed in the hope that it will be useful,
// but WITHOUT ANY WARRANTY; without even the implied warranty of
// MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
// GNU Lesser General Public License for more details.
#endregion

using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Audio;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.GamerServices;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using Microsoft.Xna.Framework.Media;
using Microsoft.Xna.Framework.Net;
using Microsoft.Xna.Framework.Storage;
using System.IO;

namespace Lightfieldretrieval
{
    class Renderer : Game
    {
        GraphicsDeviceManager graphics;
        SpriteBatch spriteBatch;

        public String filename;
        FileInfo fileinfo;
        int[] indices;              // Store the indices in a array
        Vector3[] vectors;          // And the vectors
        Vector3 center;
        
        /// <summary>
        /// Point of views, half the point of the dodecahedron
        /// </summary>
		///

		/*

		Vector3[] povs = new Vector3[] {
            new Vector3(-1.37638f, 0.0f, 0.262866f),
			new Vector3(1.37638f, 0.0f, -0.262866f),
			new Vector3(-0.425325f, -1.30902f, 0.262866f),
			new Vector3(-0.425325f, 1.30902f, 0.262866f),
			new Vector3(1.11352f, -0.809017f, 0.262866f),
			new Vector3(1.11352f, 0.809017f, 0.262866f),
			new Vector3(-0.262866f, -0.809017f, 1.11352f),
			new Vector3(-0.262866f, 0.809017f, 1.11352f),
			new Vector3(-0.688191f, -0.5f, -1.11352f),
			new Vector3(-0.688191f, 0.5f, -1.11352f),
			new Vector3(0.688191f, -0.5f, 1.11352f),
			new Vector3(0.688191f, 0.5f, 1.11352f),
			new Vector3(0.850651f, 0.0f, -1.11352f),
			new Vector3(-1.11352f, -0.809017f, -0.262866f),
			new Vector3(-1.11352f, 0.809017f, -0.262866f),
			new Vector3(-0.850651f, 0.0f, 1.11352f),
			new Vector3(0.262866f, -0.809017f, -1.11352f),
			new Vector3(0.262866f, 0.809017f, -1.11352f),
			new Vector3(0.425325f, -1.30902f, -0.262866f),
			new Vector3(0.425325f, 1.30902f, -0.262866f)
        };
		*/
 
        Vector3[] povs = new Vector3[] {
            new Vector3(1.0f, 1.0f, 1.0f),
            new Vector3(-1.0f, 1.0f, 1.0f),
            new Vector3(-1.0f, 1.0f, -1.0f),
            new Vector3(1.0f, 1.0f, -1.0f),
            new Vector3(0.0f, 0.618034f, 1.61803f),
            new Vector3(0.0f, 0.618034f, -1.61803f),
            new Vector3(1.0f, 1.61803f, 0.0f),
            new Vector3(-1.0f, 1.61803f, 0.0f),
            new Vector3(1.61803f, 0.0f, 0.618034f),
            new Vector3(-1.61803f, 0.0f, 0.618034f)
        };

		/*
		Vector3[] povs = new Vector3[] {
			new Vector3(-1.37638f, 0.0f, 0.262866f),
			new Vector3(-0.425325f, -1.30902f, 0.262866f),
			new Vector3(-0.425325f, 1.30902f, 0.262866f),
			new Vector3(1.11352f, -0.809017f, 0.262866f),
			new Vector3(1.11352f, 0.809017f, 0.262866f),
			new Vector3(-0.262866f, -0.809017f, 1.11352f),
			new Vector3(-0.262866f, 0.809017f, 1.11352f),
			new Vector3(0.688191f, 0.5f, 1.11352f),
			new Vector3(0.688191f, -0.5f, 1.11352f),
			new Vector3(-0.850651f, 0.0f, 1.11352f),
		};
		*/

        int povindex;

        Matrix worldMatrix;
        Matrix viewMatrix;
        Matrix projectionMatrix;
        VertexPositionColor[] modelVertices;
        VertexDeclaration basicEffectVertexDeclaration;
        VertexBuffer vertexBuffer;
        IndexBuffer indexBuffer;
        BasicEffect basicEffect;

        float x;
        float y;
        int s;

        public Renderer()
        {
            graphics = new GraphicsDeviceManager(this);
            graphics.PreferredBackBufferHeight = 256;
            graphics.PreferredBackBufferWidth = 256;

            Content.RootDirectory = "Content";
        }

        /// <summary>
        /// Allows the game to perform any initialization it needs to before starting to run.
        /// This is where it can query for any required services and load any non-graphic
        /// related content.  Calling base.Initialize will enumerate through any components
        /// and initialize them as well.
        /// </summary>
        protected override void Initialize()
        {			

            ///////////////////////////////////////////////////////////////////////////
            // Read mesh from a file
            ///////////////////////////////////////////////////////////////////////////
            if (filename == null || filename == "")
            {
                this.Exit();
                return;
            }

            center = Vector3.Zero;
            float distance = 0;
            using (StreamReader sr = File.OpenText(filename))
            {
                fileinfo = new FileInfo(filename);
                String s = sr.ReadLine();
                int vertexCount = Int32.Parse(s);
                //
                s = sr.ReadLine();
                int primitiveCount = Int32.Parse(s);
                //
                // Read into vertex array
                vectors = new Vector3[vertexCount];
                modelVertices = new VertexPositionColor[vertexCount];
                for (int i = 0; i < vertexCount; i++)
                {
                    s = sr.ReadLine();
                    String[] subs = s.Split(new char[] { ' ' });
                    float x = Single.Parse(subs[0]);
                    float y = Single.Parse(subs[1]);
                    float z = Single.Parse(subs[2]);
                    //
                    Vector3 v = new Vector3(x, y, z);
                    vectors[i] = v;
                    center += v;
                    modelVertices[i] = new VertexPositionColor(vectors[i], Color.Black);
                }
                center /= vertexCount;
                //
                //
                foreach (Vector3 v in vectors)
                {
                    float d = (v - center).LengthSquared();
                    if (d > distance)
                        distance = d;
                }
                distance = (float)Math.Sqrt(distance);

                // Threeangle has least indices (3)
                List<int> indexList = new List<int>(primitiveCount * 3);
                for (int i = 0; i < primitiveCount; i++)
                {
                    s = sr.ReadLine();
                    String[] subs = s.Split(new char[] { ' ' });
                    int first = Int32.Parse(subs[1]);
                    int last = Int32.Parse(subs[2]);
                    for (int j = 3; j < subs.Length; j++)
                    {
                        int curr = Int32.Parse(subs[j]);
                        indexList.Add(first);
                        indexList.Add(last);
                        indexList.Add(curr);
                        last = curr;
                    }
                }

                vertexBuffer = new VertexBuffer(graphics.GraphicsDevice, VertexPositionColor.SizeInBytes * vertexCount, BufferUsage.None);
                vertexBuffer.SetData<VertexPositionColor>(modelVertices);

                indices = indexList.ToArray();
                indexBuffer = new IndexBuffer(graphics.GraphicsDevice, typeof(Int32), indices.Length, BufferUsage.None);
                indexBuffer.SetData<Int32>(indices);
            }

            ///////////////////////////////////////////////////////////////////////////
            // Init graphics stuff
            ///////////////////////////////////////////////////////////////////////////
            worldMatrix = Matrix.Identity;
            povindex = -1;
            projectionMatrix = Matrix.CreateOrthographic(2 * distance, 2 * distance, -distance, distance );
            //
            basicEffect = new BasicEffect(graphics.GraphicsDevice, null);
            basicEffect.EnableDefaultLighting();
            basicEffectVertexDeclaration = new VertexDeclaration(graphics.GraphicsDevice, VertexPositionColor.VertexElements);

            base.Initialize();
        }

        /// <summary>
        /// LoadContent will be called once per game and is the place to load
        /// all of your content.
        /// </summary>
        protected override void LoadContent()
        {
            // Create a new SpriteBatch, which can be used to draw textures.
            spriteBatch = new SpriteBatch(GraphicsDevice);

            x = Mouse.GetState().X;
            y = Mouse.GetState().Y;
            s = Mouse.GetState().ScrollWheelValue + 1;
        }

        /// <summary>
        /// UnloadContent will be called once per game and is the place to unload
        /// all content.
        /// </summary>
        protected override void UnloadContent()
        {
            // TODO: Unload any non ContentManager content here
        }

        /// <summary>
        /// Allows the game to run logic such as updating the world,
        /// checking for collisions, gathering input, and playing audio.
        /// </summary>
        /// <param name="gameTime">Provides a snapshot of timing values.</param>
        protected override void Update(GameTime gameTime)
        {
            /*
            // Allows the game to exit
            if (GamePad.GetState(PlayerIndex.One).Buttons.Back == ButtonState.Pressed)
                this.Exit();

            //float ds = s - Mouse.GetState().ScrollWheelValue;
            s = Mouse.GetState().ScrollWheelValue + 1;
            float scale = (s > 1) ? s : 1 / s;
            //worldMatrix = Matrix.CreateScale((float)Math.Log(scale));

            if (Mouse.GetState().LeftButton == ButtonState.Pressed)
            {
                float dx = x - Mouse.GetState().X;
                float dy = y - Mouse.GetState().X;
                //
                x = Mouse.GetState().X;
                y = Mouse.GetState().Y;
                //
                const float factor = 0.01f;
                //
                worldMatrix = Matrix.CreateScale((float)Math.Pow(scale, 0.5)) * Matrix.CreateFromYawPitchRoll(x * factor, y * factor, 0.0f);
            }
            else
            {
                //worldMatrix = Matrix.CreateScale((float)Math.Pow(scale, 0.5));
            }

            // TODO: Add your update logic here
            */

            base.Update(gameTime);
        }

        /// <summary>
        /// This is called when the game should draw itself.
        /// </summary>
        /// <param name="gameTime">Provides a snapshot of timing values.</param>
        protected override void Draw(GameTime gameTime)
        {
            povindex++;
            viewMatrix = Matrix.CreateLookAt(povs[povindex] + center, center, Vector3.Up);

            GraphicsDevice.Clear(Color.White);
            graphics.GraphicsDevice.VertexDeclaration = basicEffectVertexDeclaration;
            graphics.GraphicsDevice.Vertices[0].SetSource(vertexBuffer, 0, VertexPositionColor.SizeInBytes);
            graphics.GraphicsDevice.Indices = indexBuffer;           
            graphics.GraphicsDevice.RenderState.CullMode = CullMode.None;
            //graphics.GraphicsDevice.RenderState.FillMode = FillMode.WireFrame;

            // This code would go between a device 
            basicEffect.World = worldMatrix;
            basicEffect.View = viewMatrix;
            basicEffect.Projection = projectionMatrix;
			//
			// Should resolve rendering bug
			basicEffect.TextureEnabled = false;
			basicEffect.LightingEnabled = false;
			basicEffect.VertexColorEnabled = true;
            //
            basicEffect.Begin();
            foreach (EffectPass pass in basicEffect.CurrentTechnique.Passes)
            {
                pass.Begin();
                graphics.GraphicsDevice.DrawIndexedPrimitives(PrimitiveType.TriangleList, 0, 0, vectors.Length, 0, indices.Length / 3);
                pass.End();
            }
            basicEffect.End();

            base.Draw(gameTime);

            ResolveTexture2D renderTargetTexture;
            renderTargetTexture = new ResolveTexture2D(
                graphics.GraphicsDevice,
                graphics.GraphicsDevice.PresentationParameters.BackBufferWidth,
                graphics.GraphicsDevice.PresentationParameters.BackBufferHeight,
                1,
                graphics.GraphicsDevice.PresentationParameters.BackBufferFormat);

            graphics.GraphicsDevice.ResolveBackBuffer(renderTargetTexture);
            renderTargetTexture.Save(fileinfo.FullName + povindex + ".bmp", ImageFileFormat.Bmp);
            
            if (povindex >= povs.Length - 1)
                this.Exit();
        }
    }
}
