//// At the start of each level, we construct the Game_Field, it contains all entities
//1. Determine which level we load based on the given Signal (sent through the UI by the player).
//2. Bind the level for futher reference
//3. Get player info & set gameConfig to alter difficulty
//4. Set Reward based on Difficulty

using System;
using UnityEngine;
using strange.extensions.command.impl;
using strange.extensions.context.api;

namespace virum.game
{
	public class SetupLevelCommand : Command
	{
		//Inject Game Signals
		[Inject]
		public IGameConfig gameConfig{ get; set; }
		
		[Inject]
		public IGameModel gameModel{ get; set; }

		[Inject(GameElement.GAME_FIELD)]
		public GameObject gameField{ get; set; }

		[Inject]
		public CreatePlayerSignal createPlayerSignal{ get; set; }

		[Inject]
		public CreateChestSignal createChestSignal{ get; set; }


		//Inject Signal specific variables 
		[Inject]
		public LevelNames levelID{ get; set; }

		[Inject]
		public int difficulty{ get; set; }

		//Generic functions
		private EnumFunctions enumFunction = new EnumFunctions();


		//Fire when all injections are satisfied
		public override void Execute ()
		{
			//Dispatch signal for player creation
			createPlayerSignal.Dispatch ();

			//If levelID is Random; Get random LevelName
			if(levelID == LevelNames.RANDOM)
			{
				//Int: Skip first 2 values in the enum ( Introduction & Random )
				levelID = enumFunction.GetRandomEnumValue<LevelNames>( 2 );
			}

			//Check if binding exists
			if (injectionBinder.GetBinding<GameObject> (levelID) != null){
				injectionBinder.Unbind<GameObject> (levelID);
			}

			//Setup level based on ID
			if (injectionBinder.GetBinding<GameObject> (levelID) == null)
			{
				GameObject level = MonoBehaviour.Instantiate( Resources.Load( "art/prefabs/levels/" + levelID.ToString() ) ) as GameObject;
				level.name = levelID.ToString();
				level.transform.localPosition = Vector3.zero;
				level.transform.SetParent (gameField.transform );
								
				//Bind it so we can use it elsewhere
				injectionBinder.Bind<GameObject> ().ToValue (level).ToName (levelID);
			}


			#region Difficulty Settings
			//Set difficulty based on Player Levels (50 max)
			//This is defined here because it occurs at each level start
			int totalDifficulty = gameModel.playerLevel * difficulty;

			//SetDifficulty ( enemyLevel, maxNumber of Enemies, minEnemySpawnTime, maxEnemySpawnTime );
			//Set Reward based on difficulty
			if(totalDifficulty > 120){
				gameConfig.SetDifficulty( 3, 150, 1F, 1F );
				createChestSignal.Dispatch(ChestType.Golden);

			}else if(totalDifficulty > 60){
				gameConfig.SetDifficulty( 2, 100, 3F, 2F );
				createChestSignal.Dispatch(ChestType.Silver);

			}else if(totalDifficulty > 30){
				gameConfig.SetDifficulty( 1, 50, 4F, 2F );
				createChestSignal.Dispatch(ChestType.Silver);

			}else if(totalDifficulty > 15){
				gameConfig.SetDifficulty( 1, 40, 5F, 4F );
				createChestSignal.Dispatch(ChestType.Bronze);

			}else if(totalDifficulty >= 0){
				gameConfig.SetDifficulty( 1, 15, 6F, 5F );
				createChestSignal.Dispatch(ChestType.Wooden);
			}
			#endregion

		}
	}
}

