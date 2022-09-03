using UnityEngine;

public interface IStrafer
{
	void ChooseStrafeDirectionAndDuration();
	void CoinflipStrafe();
	void ConsiderPositioning();
	void StartStrafe();
	void StopStrafing();
	void StrafeDelay(float possibletotal);
	void StrafeInDirection(Vector3 direction);
	void TryStrafe();
}