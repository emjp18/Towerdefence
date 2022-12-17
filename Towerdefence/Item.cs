using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
namespace Towerdefence
{
    internal class Item : GameObject
    {
       
        public Item(string tex, Rectangle rect, float mass, float speed, int id)
        : base(tex, rect, mass,id, speed)
        {
            
        }
      
        public override void Update(GameTime gametime)
        {
            base.Update(gametime);
        }
        public override void Draw(SpriteBatch sb)
        {
            base.Draw(sb);
        }
    }
}
