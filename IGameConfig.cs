using System;

namespace virum.game
{
	public interface IGameConfig
	{
		int initLives{ get; set; }

		int newLifeEvery{ get; set; }

		int maxNumberOfEnemies{ get; set; }

		int enemyLevel{ get; set; }

		float enemySpawnSecondsMin{ get; set; }

		float enemySpawnSecondsMax{ get; set; }

		void SetDifficulty( int _enemyLevel, int _maxEnemies, float _enemySpawnMin, float enemySpawnMax );
	}
}

