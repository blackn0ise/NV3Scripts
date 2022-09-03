using System.Collections;

public interface ISummoner
{
	IEnumerator BeginSummon();
	void DoSpawns();
	void PlaySounds();
}
