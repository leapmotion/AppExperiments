using UnityEngine;
using System.Collections;
//using DG.Tweening;

[ExecuteInEditMode]
[RequireComponent(typeof(LineRenderer))]
public class Circle : MonoBehaviour
{
    public int segments = 36;
    public float xradius = 1;
    public float yradius = 1;
    
    public float pillWidth = 0;

	private float _arcWidth = 360f;
    public float arcWidth {
        get { return _arcWidth; }
        set {
            _arcWidth = value;
            ClearPoints();
            CreatePoints();    }
    }

    [HideInInspector]
	public LineRenderer line;
    
    private GameObject go;

    void Start ()
    {
        line = GetComponent<LineRenderer>();
        CreatePoints ();
    }
   
    private void OnValidate() {
        ClearPoints();
        CreatePoints();
    }

    private void Update() {
    }

    void CreatePoints ()
    {
        line.positionCount = (segments + 1);
        line.useWorldSpace = false;

        float x;
        float y;
        float z = 0f;
       
        float angle = 20f;
       
        for (int i = 0; i < (segments + 1); i++)
        {   
            float d = 0;
            if(Mathf.Sin (Mathf.Deg2Rad * angle) >= 0) d = pillWidth/2;
            else d = -pillWidth/2;
            x = Mathf.Sin (Mathf.Deg2Rad * angle) * xradius + d;
            y = Mathf.Cos (Mathf.Deg2Rad * angle) * yradius;
                   
            line.SetPosition (i,new Vector3(x,y,z) );
                   
            angle += (_arcWidth / segments);
        }
    }

    void ClearPoints(){

        if(line == null)
        line = GetComponent<LineRenderer>();

        line.positionCount = 0;
    }

    /*public void Press(Color c){
		DOTween.Pause("circle");
		DOTween.Kill("circle");

        transform.DOScale(new Vector3(1.2f,1.2f,1.2f),0.08f).SetId("circle");
        DOVirtual.DelayedCall(0.08f, () => {
            transform.DOScale(new Vector3(1.1f,1.1f,1.1f),0.02f).SetId("circle"); 
        });

        line.material.DOColor(c, 0.1f).SetId("circle");
    }

    public void UnPress(Color c){
        DOTween.Pause("circle");
		DOTween.Kill("circle");

        transform.DOScale(Vector3.one,0.2f).SetId("circle");
        line.material.DOColor(c, 0.2f).SetId("circle");
    }

    public void SliderPress(Color c){
		DOTween.Pause("circle");
		DOTween.Kill("circle");

        transform.DOScale(new Vector3(1.2f,1.2f,1.2f),0.08f).SetId("circle");

        line.material.DOColor(c, 0.1f).SetId("circle");
    }

    public void SliderUnPress(Color c, float pH = 1){
        DOTween.Pause("circle");
		DOTween.Kill("circle");



        transform.DOScale(Vector3.one * pH,0.2f).SetId("circle");
        line.material.DOColor(c, 0.2f).SetId("circle");
    }


    public void SetColor(Color c){
        line.material.DOColor(c, 0.2f).SetId("circle");
    }
*/
}
