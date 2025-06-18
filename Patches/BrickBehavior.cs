using BepInEx.Logging;
using DunGen;
using UnityEngine;
using UnityEngine.PlayerLoop;
namespace ThrowableBrick.Patches
{
    class BrickBehavior : PhysicsProp
    {

        private Ray brickThrowRay;
        private int brickMask = 268437761;
        private RaycastHit brickHit;
        public bool isExplosive = true;
        private int health = 3;
        public override void ItemActivate(bool used, bool buttonDown = true)
        {
            base.ItemActivate(used, buttonDown);
            Debug.Log("ACTIVATED!");

            scrapValue = (int)(scrapValue * .66);
            health--;
            playerHeldBy.DiscardHeldObject(true, null, GetThrowDestination());
        }

        public override void Start()
        {
            base.Start();
        }
        public override void OnHitGround()
        {
            base.OnHitGround();
            
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
