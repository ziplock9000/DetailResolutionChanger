using UnityEngine;
using UnityEditor;

//Originally created by Kragh
//https://forum.unity.com/threads/small-terrain-scripts.43085/

public class DetailResolutionChanger : ScriptableWizard  {

	public Terrain terrain;
	public int targetResolution;
		
	[MenuItem ("Terrain/Detail Resolution Changer...")]
	
	static void createWizard() {
		
			ScriptableWizard.DisplayWizard("Select terrain to change detail resolution on", typeof (DetailResolutionChanger), "Change Resolution");
		
	}
	
	void OnWizardCreate () {

		// get all current detail Layers
		Undo.RegisterUndo (terrain.terrainData, "Detail Resolution Change");		
		
		int oldDetailResolution = terrain.terrainData.detailResolution;
		
		int detailPrototypesCount = terrain.terrainData.detailPrototypes.Length;
		
		
		int[,,] oldDetailInfo = new int[oldDetailResolution,oldDetailResolution,detailPrototypesCount];
		
		int[,,] timesWithdrawn = new int[oldDetailResolution,oldDetailResolution,detailPrototypesCount];
		
		for (int layerNumber = 0; layerNumber < detailPrototypesCount; layerNumber++) {
			
			int[,] fetchedDetailLayer = terrain.terrainData.GetDetailLayer(0,0,oldDetailResolution,oldDetailResolution,layerNumber);	
			
			//loop through layer int[,]
			
			for (int xValue = 0; xValue < oldDetailResolution; xValue++) {

				for (int yValue = 0; yValue < oldDetailResolution; yValue++) {

					//write values to store int[,,]. we need to store the values, before setting the new resolution on the terrainData object, which will erase all data.
					oldDetailInfo[xValue,yValue,layerNumber] = fetchedDetailLayer[xValue,yValue];
								
				}
	
			}
				
		}

        // change to new resolution. Let's hope we get to the end from here on, or all data is lost. Maybe I should write stored data to a file before this action?


        //terrain.terrainData.detailResolution = targetResolution;
	    terrain.terrainData.SetDetailResolution(targetResolution,terrain.terrainData.detailResolutionPerPatch);

        //write Values into new Resolution, taking density into account

        float oldResCounterX = 0f;
		float oldResCounterY = 0f;
		
		for (int layerNumber = 0; layerNumber < detailPrototypesCount; layerNumber++) {
			
			int[,] newDetailLayer = new int[targetResolution,targetResolution];
			
			//loop through new int[,] and write the stored data to it.
			
			for (int xValue = 0; xValue < targetResolution; xValue++) {

				for (int yValue = 0; yValue < targetResolution; yValue++) {

					float DetailCountKeeper = 0f;
					
					for (int resDifferenceX = 0; resDifferenceX < ((float)oldDetailResolution / targetResolution); resDifferenceX++)  {
						
						for (int resDifferenceY = 0; resDifferenceY < ((float)oldDetailResolution / targetResolution); resDifferenceY++)  {
							float calculatedAmount = 0f;
							
							if (oldDetailResolution < targetResolution)  {
								
								//	if the amount of details on one old resolution pixel can't be divided into ints above 1, we need to select only some target texture pixels to write to, and leave the rest empty. We can't use 4 pixels at 0.8 amount, as that will leave us 0 in final density!							
								
								float diffCount = (float)(System.Math.Pow(targetResolution / oldDetailResolution, 2) - timesWithdrawn[(int)oldResCounterX+resDifferenceX,(int)oldResCounterY+resDifferenceY,layerNumber]);
								float amountDivided = (float)(oldDetailInfo[(int)oldResCounterX+resDifferenceX,(int)oldResCounterY+resDifferenceY,layerNumber])/diffCount;
								
								if (amountDivided < 1f && amountDivided > 0.001f) {
									
									calculatedAmount = 1f;
									timesWithdrawn[(int)oldResCounterX+resDifferenceX,(int)oldResCounterY+resDifferenceY,layerNumber] += 1;
									oldDetailInfo[(int)oldResCounterX+resDifferenceX,(int)oldResCounterY+resDifferenceY,layerNumber] -= 1;
									
								} else if (amountDivided >= 1f) {
									
									calculatedAmount = (int)System.Math.Round(amountDivided);
									timesWithdrawn[(int)oldResCounterX+resDifferenceX,(int)oldResCounterY+resDifferenceY,layerNumber] += 1;
									oldDetailInfo[(int)oldResCounterX+resDifferenceX,(int)oldResCounterY+resDifferenceY,layerNumber] -= (int)System.Math.Round(calculatedAmount);
									
								} else {
									
									calculatedAmount = 0f;
									timesWithdrawn[(int)oldResCounterX+resDifferenceX,(int)oldResCounterY+resDifferenceY,layerNumber] += 1;
									
								}
								
							} else {
							
								calculatedAmount = (float)((((float)oldDetailResolution / targetResolution)/2f) * (float)oldDetailInfo[(int)oldResCounterX+resDifferenceX,(int)oldResCounterY+resDifferenceY,layerNumber]);
								
							}
							
							DetailCountKeeper += calculatedAmount;
												
						}
											
					}
													
					newDetailLayer[xValue,yValue] = (int)System.Math.Round(DetailCountKeeper);
												
					oldResCounterY += (float)oldDetailResolution / targetResolution;
								
				}

				oldResCounterX += (float)oldDetailResolution / targetResolution;
				oldResCounterY = 0f;
				
			}
			
			oldResCounterX = 0f;
			oldResCounterY = 0f;
			
			// put the new layer into the terrainData object, and go to next layer, if any exists!
			terrain.terrainData.SetDetailLayer(0,0,layerNumber,newDetailLayer);				
			
		}
	
		// Hey... we should be done... hope you agree.
		
	}
	
	void OnWizardUpdate ()
    {
		
		if (!terrain) {
			
			helpString = "Select a terrain object!";
			isValid = false;
			
		} else {
			
			if (targetResolution == 0) {
				
				helpString = "Type in your target resolution. Needs to be Power of two";	
				
			}
			
		}
		
		if (targetResolution > 0) {
			
			targetResolution = Mathf.ClosestPowerOfTwo (targetResolution); 
			
			
			
		}
		
		if (terrain && targetResolution >= 512) {
			
			if (terrain.terrainData.detailResolution != targetResolution) {
				
				helpString = "Go on and click that button!!!";				
				isValid = true;
				
			} else {
				
				helpString = "Your target resolution is the same as the existing \r\n resolution!";
				isValid = false;
				
			}
			
		} else if (targetResolution > 0 && targetResolution < 512) {
			
				helpString = "Your target resolution should be at least 512,\r\n as lower values are not supported by terrain objects!";
				isValid = false;
			
		}
			
	}
	
}
