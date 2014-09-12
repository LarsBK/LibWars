using System;
using System.Collections.Generic;
using Newtonsoft.Json;

namespace LibWars
{
	public class Map
	{
		[JsonProperty]
		private string
			tileset;
		[JsonProperty]
		private string[]
			entitysets;

		[JsonProperty]
		public int width { get; private set; }

		[JsonProperty]
		public int height { get; private set; }

		[JsonProperty]
		private string[]
			tiles;
		private static Dictionary<string, Map> maps = new Dictionary<string, Map> ();
		private Tileset _tileset;
		private Tile[,] _tiles;

		public Tile GetTileAt (uint x, uint y)
		{
			return _tiles [y, x];
		}

		private void Parse ()
		{
			_tiles = new Tile[height, width];
			for (int y = 0; y < tiles.Length; ++y) {
				for (int x = 0; x < tiles[y].Length; ++y) {
					_tiles [y, x] = _tileset.GetTile (tiles [y] [x]);
				}
			}
		}

		public static Map GetMap (string id)
		{
			if (! maps.ContainsKey (id)) {
			
				Client.FutureResult<Map, object> r_map =
									Client.Instance.Call<Map> ("game.getMap", new {mapid = id});
				
				maps [id] = r_map.GetResult (500);
				maps [id]._tileset = Tileset.GetTileset (maps [id].tileset);
				maps [id].Parse ();
			}
			return maps [id];
				
		}
	}
}
	
