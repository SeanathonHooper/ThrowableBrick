using BepInEx.Logging;
using UnityEngine;
namespace ThrowableBrick.Patches
{
    class BrickBehavior : PhysicsProp
    {

        private Ray brickThrowRay;
        private int brickMask = 268437761;
        public RaycastHit brickHit;
        bool hasCollided = false;
        private int health = 3;
        public override void ItemActivate(bool used, bool buttonDown = true)
        {
            base.ItemActivate(used, buttonDown);

            playerHeldBy.DiscardHeldObject(true, null, GetThrowDestination());
        }

        public override void OnHitGround()
        {
            base.OnHitGround();
            health--;
            if (health <= 0)
            {
                Landmine.SpawnExplosion(transform.position, true);
                DestroyObjectInHand(playerHeldBy);
            }
        }

        public override void EquipItem()
        {
            base.EquipItem();
            hasCollided = false;
        }
        public Vector3 GetThrowDestination()
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
