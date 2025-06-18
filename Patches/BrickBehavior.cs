using BepInEx.Logging;
using DunGen;
using GameNetcodeStuff;
using UnityEngine;
using UnityEngine.PlayerLoop;
namespace ThrowableBrick.Patches
{
    class BrickBehavior : PhysicsProp
    {

        private Ray brickThrowRay;
        private int brickMask = 268437761;
        private RaycastHit brickHit;
        PlayerControllerB playerThrower = null;
        public bool isExplosive = true;
        private int health = 3;
        private bool isThrown = false;
        public override void ItemActivate(bool used, bool buttonDown = true)
        {
            base.ItemActivate(used, buttonDown);
            Debug.Log("ACTIVATED!");

            SetScrapValue((int)(scrapValue * .66));
            health--;

            isThrown = true;
            playerThrower = playerHeldBy;
            playerHeldBy.DiscardHeldObject(true, null, GetThrowDestination());
        }

        public override void Update()
        {
            base.Update();
            if (isThrown)
            {
                Collider[] hits = Physics.OverlapSphere(transform.position, 2, 2621448, QueryTriggerInteraction.Collide);
                foreach (Collider hit in hits)
                {
                    if (hit.gameObject.layer == 3)
                    {
                        PlayerControllerB playerHit = hit.gameObject.GetComponent<PlayerControllerB>();
                        if (playerHit != playerThrower)
                        {
                            playerHit.DamagePlayer(20);
                            isThrown = false;
                        }
                    }
                    if (hit.gameObject.layer == 19)
                    {
                        EnemyAICollisionDetect enemyHit = hit.gameObject.GetComponentInChildren<EnemyAICollisionDetect>();
                        enemyHit.mainScript.HitEnemy(5, playerHeldBy, true);
                        isThrown = false;
                    }
                }
            }
        }

        public override void EquipItem()
        {
            base.EquipItem();

            string[] allLines = { "Throw: [LMB]" };

            if (base.IsOwner)
            {
                HUDManager.Instance.ChangeControlTipMultiple(allLines, true, itemProperties);
            }  
        }

        public override void OnHitGround()
        {
            base.OnHitGround();
            isThrown = false;
            
            if (health <= 0)
            {
                if (isExplosive == true)
                {
                    Landmine.SpawnExplosion(transform.position + Vector3.up, true, 5.7f, 6f, 37, 10f);
                }
                DestroyObjectInHand(playerHeldBy);
            }
        }

        private Vector3 GetThrowDestination()
        {
            Vector3 position = base.transform.position;
            Debug.DrawRay(playerHeldBy.gameplayCamera.transform.position, playerHeldBy.gameplayCamera.transform.forward, Color.yellow, 15f);
            brickThrowRay = new Ray(playerHeldBy.gameplayCamera.transform.position, playerHeldBy.gameplayCamera.transform.forward);
            position = ((!Physics.Raycast(brickThrowRay, out brickHit, 12f, brickMask, QueryTriggerInteraction.Ignore)) ? brickThrowRay.GetPoint(10f) : brickThrowRay.GetPoint(brickHit.distance - 0.05f));
            Debug.DrawRay(position, Vector3.down, Color.blue, 15f);
            brickThrowRay = new Ray(position, Vector3.down);
            if (Physics.Raycast(brickThrowRay, out brickHit, 30f, brickMask, QueryTriggerInteraction.Ignore))
            {
                return brickHit.point + Vector3.up * 0.05f;
            }
            return brickThrowRay.GetPoint(30f);
        }
    }

}
