using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;

namespace Assets.Scripts.General
{
    public static class Util
    {
        public static bool isInside(Vector2 size, Vector2 targetSize, Vector3 target, Vector3 compare)
        {
            float boxPosXMax = compare.x + size.x;
            float boxPosXMin = compare.x - size.x;

            float boxPosYMax = compare.y + size.y;
            float boxPosYMin = compare.y - size.y;

            if (target.x + targetSize.x > boxPosXMin && target.x - targetSize.x < boxPosXMax)
                if (target.y + targetSize.y > boxPosYMin && target.y - targetSize.y < boxPosYMax)
                    return true;

            return false;
        }

    }
}
