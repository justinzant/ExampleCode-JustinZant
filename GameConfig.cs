using System;
using UnityEngine;

namespace virum.game
{
	public class GameConfig : IGameConfig
	{
		//PostConstruct methods fire automatically after all injections are set
		[PostConstruct]
		public void PostConstruct()
		{
			TextAsset file = Resources.Load ("gameConfig") as TextAsset;

			var n = SimpleJSON.JSON.Parse (file.text);

			//Pllayer related
			initLives = n ["initLives"].AsInt;
			newLifeEvery = n ["newLifeEvery"].AsInt;
			maxLives = n ["maxLives"].AsInt;

			//Enemy related
			maxNumberOfEnemies = n ["maxNumberOfEnemies"].AsInt;
			enemyLevel = n ["enemyLevel"].AsInt;
			enemySpawnSecondsMin = n ["enemySpawnSecondsMin"].AsFloat;
			enemySpawnSecondsMax = n ["enemySpawnSecondsMax"].AsFloat;
		}

		#region implement IGameConfig
		public int initLives{ get; set; }

		//Unimplemented
		public int newLifeEvery{ get; set; }

		//Unimplemented
		public int maxLives{ get; set; }

		public int maxNumberOfEnemies{ get; set; }

		public int enemyLevel{ get; set; }

		public float enemySpawnSecondsMin{ get; set; }

		public float enemySpawnSecondsMax{ get; set; }

		public void SetDifficulty( int _enemyLevel, int _maxEnemies, float _enemySpawnMin, float enemySpawnMax )
		{
			enemyLevel = _enemyLevel;
			maxNumberOfEnemies = _maxEnemies;
			enemySpawnSecondsMin = _enemySpawnMin;
			enemySpawnSecondsMax = enemySpawnMax;
		}
		#endregion
	}
}

