using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HandController : MonoBehaviour
{
    // 현재 장착된 Hand형 타입 무기
    [SerializeField]
    private Hand currentHand;

    // 공격중
    private bool isAttack = false;
    private bool isSwing = false;

    private RaycastHit hitInfo; // 광선을 쐈을때 닿은 녀석의 정보를 얻어 올 수 있다.


    void Update()
    {
        TryAttack();
     
        
    }

    private void TryAttack()
    {
        // 왼쪽 버튼을 누르면
        if(Input.GetButton("Fire1"))
        {
            if(!isAttack)
            {
                // 코루틴 실행
                StartCoroutine(AttackCoroutine());

            }

        }

    }

    IEnumerator AttackCoroutine()
    {
        // 중복 실행 막음
        isAttack = true;

        currentHand.anim.SetTrigger("Attack");  // Attack 트리거 발동 (공격 애니메이션 실행)

        // 약간의 딜레이 후 isSwing 활성화
        yield return new WaitForSeconds(currentHand.attackDelayA);
        isSwing = true;

        // 공격 활성화 시점
        StartCoroutine(HitCoroutine());

        // 일정 시간이 자나면 isSwing false
        yield return new WaitForSeconds(currentHand.attackDelayB);
        isSwing = false;


        yield return new WaitForSeconds(currentHand.attackDelay - currentHand.attackDelayA - currentHand.attackDelayB);
        isAttack = false;

    }


    IEnumerator HitCoroutine()
    {
        //isSwing이 true일때 반복실행
        while(isSwing)
        {
            // 
            if(CheckObject())
            {
                // 적중한것이 있다면 꺼준다.
                isSwing = false;
                Debug.Log(hitInfo.transform.name);

            }

            yield return null;

        }

    }

    private bool CheckObject()
    {
        // 전방에 무었이 있다면 true 어떤게 충돌했는지 받아온\ㅁ
        if (Physics.Raycast(transform.position, transform.forward, out hitInfo, currentHand.range))
        {
            return true;

        }

        return false;

    }

}
