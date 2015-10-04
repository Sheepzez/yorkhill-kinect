﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;

namespace kinectApp.Entities.Germs
{
    public class BigGerm : GermBase
    {
        static int BaseId = 20223;
        static Random Rand = new Random((int)DateTime.Now.Ticks);

        public override int Height
        {
            get
            {
                return 48;
            }
        }
        const int BASEHEALTH = 250;

        public override int Width
        {
            get
            {
                return 48;
            }
        }

        public BigGerm(string aAssetName, Vector3 aPos) : this(aAssetName, aPos.X, aPos.Y, aPos.Z) { }

        public BigGerm(string aAssetName, float aX, float aY, float aZ) : base(aAssetName, aX, aY, aZ)
        {
            Id = BaseId++;
            Health = BASEHEALTH;
        }

        public override void Load(ContentManager aContentManager)
        {
            base.Load(aContentManager);
        }

        public override void Unload()
        {
            base.Unload();
        }

        public override void Update(GameTime aGameTime)
        {
            if ((PosX < -65 || PosX > 1950) || (PosY < -65 || PosY > 1200) || Health < 0)
            {
                IsDead = true;
                return;
            }

            if (HasBeenHit)
            {
                if ((DateTime.Now - iHitTime).Milliseconds > WAITTIME)
                {
                    HasBeenHit = false;
                }
            }

            int DirX, DirY;
            if (!beenToTopHalfOfScreen)
            {
                DirY = Rand.Next(100) < 80 ? Rand.Next(8) * -1 : Rand.Next(2);
                if (PosY < Program.game.depthHeight/3) beenToTopHalfOfScreen = true;
            }
            else
            {
                DirY = Rand.Next(100) < 20 ? Rand.Next(2) * -1 : Rand.Next(8);
            }
            DirX = Rand.Next(0, 5) - 2;

            PosY += DirY;
            PosX += DirX;

        }

        public override void Draw(SpriteBatch aSpriteBatch)
        {
            int x1 = (int)PosX;
            int y1 = (int)PosY;
            var rec = new Rectangle(x1, y1, Width, Height);

            if (HasBeenHit)
            {
                aSpriteBatch.Draw(Texture, rec, Color.DarkOrange);
            }
            else
            {
                aSpriteBatch.Draw(Texture, rec, Color.White);
            }
        }
    }
}
