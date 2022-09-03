public interface IChargingUnit
{
	void Charge();
	void EndCharge();
	void DecrementTimers();
	void HandleMovement();
}
