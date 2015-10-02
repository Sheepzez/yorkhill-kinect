﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace kinectApp.Entities
{
    public abstract class BaseEnitiy : IEntity
    {
        private Vector3 _pos;
        private SpriteBatch _spriteBatch;

        #region Constructors
        public BaseEnitiy() : this(0, 0, 0) { }

        public BaseEnitiy(Vector3 aPos) : this(aPos.X, aPos.Y, aPos.Z) { }

        public BaseEnitiy(float aX, float aY, float aZ)
        {
            _pos = new Vector3(aX, aY, aZ);
        }
        #endregion

        /// <summary>
        /// Gets or sets the internal spritebatch of the entity
        /// </summary>
        protected SpriteBatch SpriteBatch
        {
            get { return _spriteBatch; }
            set { _spriteBatch = value; }
        }

        #region Position Code
        /// <summary>
        /// Gets the Position of the Germ in the Game
        /// </summary>
        public Vector3 Pos
        {
            get { return _pos; }
        }

        /// <summary>
        /// Gets or Sets the X position of the Germ
        /// </summary>
        public float PosX
        {
            get { return Pos.X; }
            set { _pos.X = value; }
        }

        /// <summary>
        /// Gets or Sets the Y position of the Germ
        /// </summary>
        public float PosY
        {
            get { return Pos.Y; }
            set { _pos.Y = value; }
        }

        /// <summary>
        /// Gets or Sets the Z position of the Germ
        /// </summary>
        public float PosZ
        {
            get { return Pos.Z; }
            set { _pos.Z = value; }
        }

        #endregion

        public abstract void Draw(GameTime aGameTime);
        public abstract void Update(GameTime aGameTime);

        public abstract void Load(SpriteBatch aSpriteBatch);
        public abstract void Unload();
    }
}