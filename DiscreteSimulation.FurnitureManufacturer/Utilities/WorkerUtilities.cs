using Simulation;

namespace DiscreteSimulation.FurnitureManufacturer.Utilities;

public class WorkerUtilities
{
    public static readonly IComparer<MyMessage> RequestsComparator = Comparer<MyMessage>.Create((a, b) =>
    {
    	// 1. Dokončenie objednávky/nábytku (montáž kovaní)
    	if (a.Furniture.CurrentOperationStep == FurnitureOperationStep.Folded && 
    	    b.Furniture.CurrentOperationStep != FurnitureOperationStep.Folded)
    	{
    		// a
    		return -1;
    	}
    	
    	if (a.Furniture.CurrentOperationStep != FurnitureOperationStep.Folded && 
    	    b.Furniture.CurrentOperationStep == FurnitureOperationStep.Folded)
    	{
    		// b
    		return 1;
    	}
    	
    	if (a.Furniture.CurrentOperationStep == FurnitureOperationStep.Folded && 
    	    b.Furniture.CurrentOperationStep == FurnitureOperationStep.Folded)
    	{
    		// 2. Najstaršia objednávka
    		if (a.Furniture.Order.ArrivalTime < b.Furniture.Order.ArrivalTime)
    		{
    			return -1;
    		}
    		
    		if (a.Furniture.Order.ArrivalTime > b.Furniture.Order.ArrivalTime)
    		{
    			return 1;
    		}
    		
    		return 0;
    	}
    	
    	// 2. Najstaršia objednávka
    	if (a.Furniture.Order.ArrivalTime < b.Furniture.Order.ArrivalTime)
    	{
    		return -1;
    	}
    	
    	if (a.Furniture.Order.ArrivalTime > b.Furniture.Order.ArrivalTime)
    	{
    		return 1;
    	}
    	
    	// 3. Predchádzajúci technologický krok bol dokončený najskôr
    	if (a.Furniture.OperationStepEndTime < b.Furniture.OperationStepEndTime)
    	{
    		return -1;
    	}
    	if (a.Furniture.OperationStepEndTime > b.Furniture.OperationStepEndTime)
    	{
    		return 1;
    	}
    	
    	return 0;
    });
}


