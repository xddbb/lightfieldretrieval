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
        Vector3 centroid;
        
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

		Random random;
		Matrix[] rotations;

        int povindex;
		int rotindex;

        Matrix worldMatrix;
        Matrix viewMatrix;
        Matrix projectionMatrix;
        VertexPositionColor[] modelVertices;
        VertexDeclaration basicEffectVertexDeclaration;
        VertexBuffer vertexBuffer;
        IndexBuffer indexBuffer;
        BasicEffect basicEffect;

        public Renderer()
        {
            graphics = new GraphicsDeviceManager(this);
            graphics.PreferredBackBufferHeight = 256;
            graphics.PreferredBackBufferWidth = 256;

            Content.RootDirectory = "Content";
        }

		
		/// <summary>
		/// Pseudo random orinentation
		/// </summary>
		public Matrix GetRandomOrientation()
		{
			//////////////////////////////////////////////////////////////////////
			// Get a vector, uniformly distributed on a unit sphere by cutting
			// off the corners of the cube. Then create a orthonormal basis.
			// Using the random local field ensures pseudo randomnes
			//////////////////////////////////////////////////////////////////////
			Vector3 v;
			do
			{
				v = new Vector3(
						2.0f * (float)random.NextDouble() - 1.0f,
						2.0f * (float)random.NextDouble() - 1.0f,
						2.0f * (float)random.NextDouble() - 1.0f);
			} while (v.Length() > 1.0f);
			//
			v.Normalize();
			Vector3 t = Vector3.Up;
			Vector3 u = Vector3.Cross(v, t);
			u.Normalize();
			Vector3 w = Vector3.Cross(u, v);
			w.Normalize();

			Matrix rot = Matrix.Identity;			
			rot.Forward = v;
			rot.Left = u;
			rot.Up = w;
			return rot;
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
            // Read mesh from a file and generate veretex and index buffers on the GPU
            ///////////////////////////////////////////////////////////////////////////

            if (filename == null || filename == "")
            {
                this.Exit();
                return;
            }

            centroid = Vector3.Zero;
            float distance = 0;
            using (StreamReader sr = File.OpenText(filename))
            {
                fileinfo = new FileInfo(filename);
                String s = sr.ReadLine();
                int vertexCount = Int32.Parse(s);
                //
                s = sr.ReadLine();
                int primitiveCount = Int32.Parse(s);
                

				//////////////////////////////////////////////////////////////////////
				// Read the vertices from the file 
				//////////////////////////////////////////////////////////////////////
                // Read into vertex array
                vectors = new Vector3[vertexCount];
                modelVertices = new VertexPositionColor[vertexCount];
                for (int i = 0; i < vertexCount; i++)
                {
                    s = sr.ReadLine();
                    String[] subs = s.Split(new char[] { ' ' });

					float x = 0.0f, y = 0.0f, z = 0.0f;	// Must init here, hence the 0-s
					Vector3 v;
					try
					{
#if !COMMAS
						x = Single.Parse(subs[0].Replace('.',','));
						y = Single.Parse(subs[1].Replace('.',','));
						z = Single.Parse(subs[2].Replace('.',','));
#else
						x = Single.Parse(subs[0]);
						y = Single.Parse(subs[1]);
						z = Single.Parse(subs[2]);
#endif
					}
					catch (FormatException ex)
					{
						// Model bug!! Few models have a (-1.#QNAN -1.#QNAN -1.#QNAN) vector
						// and will be treated like (-1, -1, -1)
						x = -1.0f;
						y = -1.0f;
						z = -1.0f;
					}
                    //
                    v = new Vector3(x, y, z);
                    vectors[i] = v;
                    modelVertices[i] = new VertexPositionColor(vectors[i], Color.Black);
                }


				//////////////////////////////////////////////////////////////////////
				// Create the triangles (index buffer data) and calculate the center of mass
				//////////////////////////////////////////////////////////////////////
                // Thriangle has least indices (3)
                List<int> indexList = new List<int>(primitiveCount * 3);
				centroid = Vector3.Zero;									// Init the center to Zero
				float totalarea = 0;
                for (int i = 0; i < primitiveCount; i++)
                {
                    s = sr.ReadLine();
                    String[] subs = s.Split(new char[] { ' ' });
                    int first = Int32.Parse(subs[1]);
                    int last = Int32.Parse(subs[2]);
					// Center of the polyface, accumulated over the triangles of the polyface
					Vector3 polycenter = Vector3.Zero;
                    for (int j = 3; j < subs.Length; j++)
                    {
                        int curr = Int32.Parse(subs[j]);
                        indexList.Add(first);
                        indexList.Add(last);
                        indexList.Add(curr);                        
						
						// Get the three vectors of the triangle
						Vector3 a = vectors[first];
						Vector3 b = vectors[curr];
						Vector3 c = vectors[last];
						// Get the center of it	and scale it with the area
						float area = Vector3.Cross((b - a), (c - a)).Length() * 0.5f;
						totalarea += area;
						polycenter += (area / 3.0f) * (a + b + c);						

						// Ready for next loop
						last = curr;
                    }
					polycenter /= subs.Length - 3; //  scale it with the no. of triangles
					centroid += polycenter;
                }
				centroid /= totalarea;


				//////////////////////////////////////////////////////////////////////
				// Get the distance to the most distant vertex
				//////////////////////////////////////////////////////////////////////
				foreach (Vector3 v in vectors)
				{
					float d = (v - centroid).LengthSquared();
					if (d > distance)
						distance = d;
				}
				distance = (float)Math.Sqrt(distance);

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
            projectionMatrix = Matrix.CreateOrthographic(2 * distance, 2 * distance, -2 * distance, 2 * distance );
            //
            basicEffect = new BasicEffect(graphics.GraphicsDevice, null);
            basicEffect.EnableDefaultLighting();
            basicEffectVertexDeclaration = new VertexDeclaration(graphics.GraphicsDevice, VertexPositionColor.VertexElements);

			//////////////////////////////////////////////////////////////////////
			// Pseudo random rotations
			//////////////////////////////////////////////////////////////////////
			random = new Random(filename.GetHashCode());	// always the same seed for the same modelpath
			rotations = new Matrix[10];
			// A lot of models of the same class are roatated the same,
			// so we keep one initial roatation well
			rotations[0] = Matrix.Identity;
			for (int i = 1; i < rotations.Length; i++)
			{
				rotations[i] = GetRandomOrientation();
			}
			rotindex = 0;

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
            // No update logic
            base.Update(gameTime);
        }

        /// <summary>
        /// This is called when the game should draw itself.
        /// </summary>
        /// <param name="gameTime">Provides a snapshot of timing values.</param>
        protected override void Draw(GameTime gameTime)
        {
            povindex++;
            viewMatrix = Matrix.CreateLookAt(povs[povindex], Vector3.Zero, Vector3.Up);

            GraphicsDevice.Clear(Color.White);
            graphics.GraphicsDevice.VertexDeclaration = basicEffectVertexDeclaration;
            graphics.GraphicsDevice.Vertices[0].SetSource(vertexBuffer, 0, VertexPositionColor.SizeInBytes);
            graphics.GraphicsDevice.Indices = indexBuffer;           
            graphics.GraphicsDevice.RenderState.CullMode = CullMode.None;		
            //graphics.GraphicsDevice.RenderState.FillMode = FillMode.WireFrame;

            // Send matrices to the shader program
			//basicEffect.World = rotations[rotindex] * Matrix.CreateTranslation(-center);		//Bring to origin and rotate
			basicEffect.World = Matrix.CreateTranslation(-centroid) * rotations[rotindex];
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
            renderTargetTexture.Save(fileinfo.FullName + "_LF" + rotindex + "_IMG" + povindex + ".png", ImageFileFormat.Png);

			if (povindex >= povs.Length - 1)
			{
				rotindex++;
				povindex = -1;				
			}
			if(rotindex >= rotations.Length)
				this.Exit();
        }
    }
}
