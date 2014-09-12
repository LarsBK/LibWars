using System;
using System.Collections.Generic;

namespace LibWars
{
	public class Tileset
	{
	
		private static Dictionary<string, Tileset> tilesets = new Dictionary<string, Tileset> ();
		private Dictionary<string, Tile> tileTypeMap = new Dictionary<string, Tile> ();
		
		private Tileset (Dictionary<string, Tile> map)
		{
			tileTypeMap = map;
		}
		
		public Tile GetTile (char type)
		{
			return tileTypeMap [type.ToString ()];
		}
		
		public static Tileset GetTileset (string id)
		{
			if (! tilesets.ContainsKey (id)) {
		
				Client.FutureResult<Dictionary<string, Tile>, object> r_tile_map =
									Client.Instance.Call<Dictionary<string, Tile>> ("game.getTileset", new {tileset = id});
			
				tilesets [id] = new Tileset (r_tile_map.GetResult (500));
			}
			return tilesets [id];
			
		}
		
	}

	public class Tile
	{
		public class Color
		{
			public float r, g, b;
		}
		public string name;
		public float terrain;
		public float hitprob;
		public float water;
		public float camo;
		public Color realColor;
		public string color;
	}


}
