using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MovingObject : MonoBehaviour
{
    public float speed;

    private Vector3 vector; // save 3 factors

    public float runSpeed; // run speed
    private float applyRunSpeed; // if you walk, this will be 0. if you run, this will be run speed.

    public int walkCount;
    private int currentWalkCount;
    // if, speed = 2.4, walkCount = 20 -> 한 번 방향키를 누를 때 마다 2.4 * 20 = 48픽셀씩 이동시키겠다는 의미.
    // 그러니까, 한 번에 48픽셀씩 움직이게 하고 싶다. 스르륵 움직여서 48픽셀 이동하게끔 보이게 하고 싶은 거다. 
    // 자, 다시. 한 번 움직일 때 2.4픽셀씩 움직이는 게 아니라, 48픽셀씩 움직이도록 하고 싶다. 순간이동이 아니라 키를 한 번 누르면 스르륵 2.4픽셀씩 20번 움직여서 도달하도록 하고 싶은 거다.
    // 우리가 20번 누르는 건 노가다니까, while문을 돌려서 자동으로 2.4픽셀씩 20번 움직이게 하는 것이다. 그래서 키 한 번 누르면 스르륵 48픽셀 움직이도록.
    // 근데 문제는 while문이 너무 빨리 이루어져서 스르륵 이동하는 게 아니라 마치 순간이동 하는 것 처럼 보인다. 또한 while문이 끝나기도 전에 input이 또 이뤄져서 48픽셀씩 두 세 번 이동하는 것 처럼 보이게 되는 것이다.
    // 이 문제를 해결하기 위해서 대기를 걸어준다. 함수 내에서 해결 할 수도 있지만 더 좋은 방법인 Couroutine(코루틴)을 사용한다.
    // 함수가 실행되다가 코루틴이 실행되면, 함수와 코루틴이 동시에 실행된다. 유니티는 다중처리를 지원하지 않지만 이 방법을 써서 다중처리인 것처럼 보이게 하는 것이다.
    // 코루틴은 대기하는 명령어를 가지고 있기 때문에 쉽게 대기시키고 진행시킬 수 있다. 따라서 코루틴은 활용도가 무궁무진함.
    // 따라서 코루틴을 써서 2.4픽셀씩 이동시키는 모습을 보여주겠다.
    // 근데 코루틴만 넣어주면 똑같은 문제가 발생한다. 이유는 방향키를 누르는 순간 너무 빠르게 실행되어 코루틴이 여러 개가 실행되기 때문이다.
    // 이걸 막아야 한다. 코루틴 반복실행을 방지하기 위해 변수를 설정한다. -> 여기선 canMove라는 이름을 주겠음.

    private bool canMove = true;
    private bool applyRunFlag = false;
    //여기까지 했다. 20픽셀씩 움직인다. 한 번 눌렀는데 두 세 번 움직이는 일은 없다. 근데 2.4픽셀씩 20번 움직이게 하고 싶다.
    //그러기 위해선 yield return new WaitForSeconds(0.02f);를 반복문 안에 넣어준다.
    //그리고 시간을 0.01초로 한다. 원래는 루프 밖에서 1초였는데 루프를 20번 돌게 되니까 모두 합해서 0.2초가 되겠지.
    //와 대박. 근데 여기서 문제가 또 있다. 쉬프트를 누르면 한 칸 이상 움직인다.
    //한 칸 이상 씩 움직이는 이유는 (speed + applyRunSpeed)*20 이렇게 움직이기 때문이다.
    //따라서 현재는 일반 속도와 달리기가 2배 차이가 난다. 따라서 currentWalkCount를 2배씩 빠르게 증가시킨다.
    //currentWalkCount가 1씩 증가한다면 총 20번 루프를 돌 것이다. 하지만 2씩 증가한다면 그 것의 1/2인 10번 루프를 돌 것이다.
    //따라서 두 배 가는 걸 루프를 반비례하게 1/2번 도는 것으로 해서 해결한 것이다.
    //다시 정리. 원래 속도로는 2.4픽셀씩 20번 이동했다. 하지만 쉬프트를 눌렀을 때는 (2.4+2.4)픽셀씩 20번 이동한다. 따라서 10번 이동하게 바꿔주어 이동 픽셀을 맞췄다.
    //이걸 막아보자. 한 가지 변수가 필요하다. 여기서 변수 이름을 applyRunFlag라고 하겠다.

    private Animator animator;

    // Start is called before the first frame update
    void Start()
    {
        animator = GetComponent<Animator>(); 
        // 유니티 프로그램에서 a라는 객체에(여기선 Character 1_1) 에니메이터를 생성했을 때,
        // 자동으로 컴포넌트(이것 저것 구성 요소들)가 생성된다. 이를 통제하기 위해 getComponent를 사용하여 해당 컴포넌트를 가져옴.

    }

    IEnumerator MoveCoroutine(){

        while(Input.GetAxisRaw("Vertical") != 0 || Input.GetAxisRaw("Horizontal") != 0) { // (2)반복문 안에 집어넣어준 모습.
        //(3) 이렇게 되면 코루틴은 한 번만 실행되고 이 코루틴 안에서 입력이 이뤄지면 계속 이동이 이뤄짐.
        //(4) 코루틴을 돌면서도 계속 입력이 이뤄질 것이다. 그러면 한 루틴이 끝나면 다음 루틴을 시작하는 게 아니라, 코루틴이 실행되고 있는 동안에도 입력을 받아서 계속적으로 루틴을 돌릴 수 있게 되는 것이다.
        //(5) 그러면 와일문이 끝나기 전까지는 계속 애니메이션이 실행될거다.
        //(7) 근데 또 오류가 있다. 위아래를 동시에 누르면 뭔가 어색하게 걷는다. 이를 고쳐보자.

            if(Input.GetKey(KeyCode.LeftShift)) {
                applyRunSpeed = runSpeed; 
                applyRunFlag = true;
            } else {
                applyRunSpeed = 0; 
                applyRunFlag = false;
            }

            vector.Set(Input.GetAxisRaw("Horizontal"), Input.GetAxisRaw("Vertical"), transform.position.z);

            if(vector.x != 0) //(8)
                vector.y = 0; // (8)만약 vector.x = 1 이라면 우측으로 이동한다는 거다. 이 때, vector.y = 0으로 만든다는 코드다. 굳이 오른쪽으로 가고 있는데 다른 정보는 필요 없다. 위아래도 마찬가지다.


            animator.SetFloat("DirX", vector.x);
            animator.SetFloat("DirY", vector.y);
            animator.SetBool("Walking", true);

            while(currentWalkCount < walkCount){
                if(vector.x != 0)
                {
                    transform.Translate(vector.x * (speed + applyRunSpeed), 0, 0);
                } else if(vector.y != 0) 
                {
                    transform.Translate(0, vector.y * (speed + applyRunSpeed), 0);
                }
                if(applyRunFlag) {
                    currentWalkCount++;
                }
                currentWalkCount++;
                yield return new WaitForSeconds(0.02f);
            }

            currentWalkCount = 0;

        }
        animator.SetBool("Walking", false);
        canMove = true;
    }
    //     if(Input.GetKey(KeyCode.LeftShift)) {
    //         applyRunSpeed = runSpeed; // if you push "shift" key, applyRunSpeed is runSpeed.
    //         applyRunFlag = true;
    //     } else {
    //         applyRunSpeed = 0; // But, applyRunSpeed will be 0 if you release "shift" button.
    //         applyRunFlag = false;
    //     }

    //     vector.Set(Input.GetAxisRaw("Horizontal"), Input.GetAxisRaw("Vertical"), transform.position.z);

    //     animator.SetFloat("DirX", vector.x); // 유니티에서 파라미터 값으로 DirX와 DirY, 그리고 Walking을 설정했었다. 그래서 위쪽 키가 눌리면 DirY에 1이 대입되고, 아래쪽 키가 눌리면 -1이 대입되도록 지정했다. 오른쪽 왼쪽은 각각 DirX에 1, -1을 지정했었음. 그 변수를 vector.x, vector.y 값으로 받는 것이다.
    //     //좌키가 눌리면 Input.GetAxisRaw로 -1이 들어올 것이고, 그걸 vector.x가 받아서 "DirX"에 주면 유니티에서 DirX에 -1이 들어있네. 그러면 Character_walking_left 애니메이션을 실행시켜야겠군. 이런 식으로 되는 것 같음.
    //     animator.SetFloat("DirY", vector.y);
    //     animator.SetBool("Walking", true);
    //     //이렇게 하면 멈춰있던 Standing tree에서 Walking tree로 상태 이전이 일어나게 됨.
    //     // 이렇게 설정해주면 애니메이션이 실행되면서 실제 이동은 아래의 while 반복문을 통해 실행되게 될 것이다.


    //     while(currentWalkCount < walkCount){
    //         if(vector.x != 0)
    //         {
    //             transform.Translate(vector.x * (speed + applyRunSpeed), 0, 0); // when you run, applyRunSpeed will be added.
    //         } else if(vector.y != 0) 
    //         {
    //             transform.Translate(0, vector.y * (speed + applyRunSpeed), 0);
    //         }
    //         if(applyRunFlag) {
    //             currentWalkCount++; //만약 쉬프트가 눌리면 currentWalkCount는 2씩 증가하고 눌리지 않으면 1씩 증가함.
    //         }
    //         currentWalkCount++;
    //         yield return new WaitForSeconds(0.02f);
    //     }

    //     currentWalkCount = 0;

    //     animator.SetBool("Walking", false); // 이동이 끝나면 다시 서있는 애니메이션으로 바꿔준다. Walking tree에서 Standing tree로 상태전이가 일어남.
    //     // yield return new WaitForSeconds(1f); // wait for one seconds. This is couroutine. 반복문 안에 넣어줌
    //     // 여기까지 했고 걸었는데 방향키를 딱 누를 때 걷는 모션이 제 때 딱 실행이 안 되고 멈추는 순간에도 걷는 모션을 함.
    //     // 이유는 (Walking Tree와 Standing Tree의 상태 전이가 일어날 때, 대기 혹은 모션이 끝날 때 까지 기다려주는 기능이 유니티 내 설정(Walking Tree와 Standing Tree의 상태전이 화살표를 누르고 접혀있는 Settings를 펼치면 확인할 수 있음)에 체크되어 있기 때문이다.)
    //     // 여기서 Has Exit Time을 클릭 해제해주고, 전이 대기(Transition Duratior)를 0으로 만들어주면 된다. 이러면 상태가 변할 때 마다 즉시 반영이 됨.
    //     // 확실히 반응 속도는 빨라졌지만 문제가 또 있다. 두 타일 이상 걸으면 걷는 느낌이 안 난다. 한 발로만 걷는 느낌. 만든 코드를 보면 코루틴이 한 번 실행되고나선 코루틴 실행이 끝나야 다시 입력 처리를 받게 만들어 두었다. 그러면 한 번 걷고 다시 멈추고 이런 식이 되어 버린다. 따라서 이 과정을 좀 수정해야 한다.
            // 즉, 두 타일을 걷게 되면 한 타일을 걷고 멈춘다. 그리고 다시 걷고 멈춘다. 한 타일을 걸을 땐 잘 되다가 멈추고 다시 걸으려니까 같은 발로 걷기 시작하여 한 발로만 걷는 듯한 느낌을 주는 것이다.
    //     // 어떻게 수정하느냐. 코루틴 안에 다시 반복문을 굴린다. (1)
    //     canMove = true; // 작업이 끝나면 방향키 처리가 가능하도록 만들어주기. canMove가 계속 false 상태로 있으면 방향키를 눌러도 if(canmove)에서 막혀버림.
    // }

    // Update is called once per frame
    void Update()
    {
        if(canMove) {
            if(Input.GetAxisRaw("Horizontal") != 0 || Input.GetAxisRaw("Vertical") != 0) {
                canMove = false; // 이 구간이 두 번 이상 실행 안 되게 막아줌.
                //(6)이런 의문이 순간 생겼다. 위에서 어차피 또 Input.GetAxisRaq 해서 키 받을텐데 여기서 키 못 받게 해봤자 의미가 있나?
                //(6)근데 의미가 있다. 코루틴이 여러번 실행되지 못하게 해주는 것이다.
                //(6)만약 이 코드가 없다면 코루틴이 여러 번 실행될 것이고 손가락을 뗐는데도 몇 번 더 걸을 거다. 코루틴 두~세 개가 실행되기 때문이다. (추측)
                //(6)위에서 입력을 받으면 한 코루틴이 계속 실행된다. 위의 코드와 여기서의 코드 모두 입력을 받지만 위의 코드는 코루틴을 계속 돌리기 위한 것이고 여기서의 코드는 코루틴이 여러 개 돌아가는 걸 막기 위한 것인 것 같다.
                //(6)근데 한 가지 의문. 왜 입력이 두 세 번씩 받아지는거지? 그렇게 되면 얼불춤같은 게임은 만들어지지 못했을텐데.
                StartCoroutine(MoveCoroutine());
            }
        }
    }

    //20:29부터 실행하기
}