﻿using System.Collections;
using UnityEngine;

namespace RN.Network.SpaceWar.Fx
{
    public class LaserFx : ActorFx<LaserSpawner>
    {
        LineRenderer lineRenderer;
        bool raycastAll = true;
        LayerMask layerMask;
        float distance;
        float startOffset;
        public override void onCreateFx(LaserSpawner actorSpawner)
        {
            raycastAll = actorSpawner.raycastAll;
            lineRenderer = GetComponentInChildren<LineRenderer>();

            if (raycastAll)
            {
                lineRenderer.SetPosition(lineRenderer.positionCount - 1, new Vector3(0f, 0f, actorSpawner.distance));

                enabled = false;
            }
            else
            {
                layerMask = actorSpawner.layerMask;
                distance = actorSpawner.distance;
                startOffset = actorSpawner.startOffset;
            }
        }

        public override void onDestroyFx(LaserSpawner actorSpawner)
        {
            if (lineRenderer != null)
            {
                StartCoroutine(playDestroyFx());

                var @as = GetComponent<AudioSource>();
                if (@as != null && @as.isPlaying)
                    StartCoroutine(@as.fadeOut());
            }

            //laser效果会自己删除
        }

        public float destroyFxTime = 1.5f;
        IEnumerator playDestroyFx()
        {
            //
            var ps = GetComponentInChildren<ParticleSystem>();
            if (ps != null)
            {
                ps.Stop();
            }


            //
            var b = lineRenderer.startColor;
            var e = b;
            e.a = 0f;
            var wb = lineRenderer.widthMultiplier;
            var we = 0f;
            foreach (var t in new TimeEquation().linear.play(destroyFxTime))
            {
                lineRenderer.startColor = Color.Lerp(b, e, t);
                lineRenderer.endColor = lineRenderer.startColor;
                lineRenderer.widthMultiplier = Mathf.Lerp(wb, we, t);
                yield return this;
            }


            //
            if (ps != null)
            {
                while (ps.isPlaying)
                {
                    yield return this;
                }
            }


            //
            this.destroyGO();
        }

        private void FixedUpdate()
        {
            if (raycastAll == false)
            {
                var ray = new Ray { origin = transform.position + transform.forward * startOffset, direction = transform.forward };

                var endPoint = new Vector3(0f, 0f, distance);
                if (Physics.Raycast(ray, out RaycastHit hitInfo, distance - startOffset, layerMask))
                {
                    endPoint = transform.InverseTransformPoint(hitInfo.point);
                }

                lineRenderer.SetPosition(lineRenderer.positionCount - 2, Vector3.LerpUnclamped(lineRenderer.GetPosition(lineRenderer.positionCount - 3), endPoint, 0.95f));
                lineRenderer.SetPosition(lineRenderer.positionCount - 1, endPoint);
            }
        }



        /// <summary>
        /// 在服务器端 需要删除渲染和没用的组件...
        /// </summary>
        /// <param name="actorSpawner"></param>
        /*public override void RemoveFxsInServer(ActorSpawner actorSpawner)
        {
            RemoveFx._RemoveFxsInServer(this, actorSpawner, true);
        }*/
    }
}