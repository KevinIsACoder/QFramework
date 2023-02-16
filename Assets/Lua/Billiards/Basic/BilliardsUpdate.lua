local BilliardsUpdate = class("BilliardsUpdate")

-- local sb = {}

function BilliardsUpdate:FixedUpdate()
    -- local deltaTime = math.fixed3(Time.fixedDeltaTime)
    -- 只有击球移动阶段才计算
    if not BilliardsBattle.CheckRoundPhase(BillardsType.RoundPhase.BALL_MOVING) then
        return
    end

    -- math.fixed3(Time.fixedDeltaTime)
    self:UpdateMove(false)
end

function BilliardsUpdate:GetCollideVolume(vx, vy)
    return math.min(math.max(math.abs(vx), math.abs(vy)) / BillardsCfg.BilliardsValueCfg.MAX_F, 1)
end

function BilliardsUpdate:UpdateMove(test)
    local deltaTime = 0.02
    local balls = BilliardsBattle.GetLeftBalls()
    for i = 1, #balls do
        local ball = balls[i]
        if ball and ball:IsLive() and not ball:IsStopped() then
            local vx, vy = ball:GetVel()
            local posX, posY = ball:GetPosition()
            local slices = math.max(math.ceil(math.fixed3(math.max(math.abs(vx), math.abs(vy)) * deltaTime)), 1)
            local px, py = posX, posY 
            local tx, ty = vx, vy
            -- print("BilliardsUpdate:FixedUpdate:+++++++++++++ cue, posx, posy, vx, vy", ball:GetNum(), posX, posY, vx, vy)
            -- 拆分时间片
            for step = 1, slices do
                px, py, tx, ty = self:SimulatePositionAndVelocity(posX, posY, vx, vy, math.fixed3((deltaTime * step) / slices))
                -- if ball:GetNum() == 0 then
                --     sb[#sb + 1] = string.format("BilliardsUpdate:----------FixedUpdate:+++++++++++, %s, %s, %s, %s, %s, %s, %s, %s, %s, %s, %s \n", px, py, tx, ty, posX, posY, vx, vy, step, slices, math.fixed3((deltaTime * step) / slices))
                --     -- print("BilliardsUpdate:----------FixedUpdate:+++++++++++", px, py, tx, ty, posX, posY, vx, vy, step, slices, math.fixed3((deltaTime * step) / slices))
                -- end
                -- if ball:GetNum() == 1 or ball:GetNum() == 13 then
                -- print("BilliardsUpdate:FixedUpdate:+++++++++++++ step, cue, posx, posy, vx, vy, px, py, tx, ty::::", step, ball:GetNum(), posX, posY, vx, vy, px, py, tx, ty)
                -- end
                -- 检查是否进袋
                if not ball:IsStopped() and self:CollidesPocket(ball, px, py) then
                    if not test then
                        BilliardsAudio.PlaySound("Ball_in_01")
                    end
                    goto continue
                end
                -- 检查是否与球碰撞
                for j = 1, #balls do
                    local hit = balls[j]
                    -- print("FixedUpdate:CollidesBall:+++++++++++++ step, cue, hit, px, py, tx, ty::::", step, ball:GetNum(), hit:GetNum(), px, py, tx, ty)
                    if hit and ball ~= hit and hit:IsLive() then
                        local touched, ox, oy = self:CollidesBall(ball, hit, px, py, tx, ty)
                        if touched then
                            -- print("FixedUpdate:CollidesBall:++++++1+++++++ step, cue, hit, px, py, ox, oy::::", step, ball:GetNum(), hit:GetNum(), px, py, ox, oy, (px - posX), (py - posY))
                            -- print("")
                            px = px + ox
                            py = py + oy
                            if not test then
                                BilliardsAudio.PlaySound("Ball_hit_01", self:GetCollideVolume(tx, ty))
                            end
                            -- sb[#sb + 1] = string.format("BilliardsUpdate:----------FixedUpdate:+++++touched++++++, %s, %s, %s, %s, %s \n", touched, px, py, ball:GetVel())
                            -- -- print("BilliardsUpdate:----------FixedUpdate:+++++touched++++++", touched, px, py, ball:GetVel())
                            -- print(table.concat(sb))
                            -- sb = {}
                            goto done
                        end
                    end
                end
                -- 检查是否与桌子碰撞
                if not ball:IsStopped() then
                    -- 撞到洞口的边缘 或者 桌子边缘
                    local touched, ox, oy = self:CollidesWall(ball, px, py, tx, ty)
                    if touched then
                        px = px + ox
                        py = py + oy
                        
                        if not test then
                            BilliardsAudio.PlaySound("Ball_rail_01", self:GetCollideVolume(tx, ty))
                        end
                        goto done
                    end
                end
            end
            
            -- 正常运动，未与任何有碰撞，不改变方向
            ball:SetVel(tx, ty)

            :: done ::

            -- 调整当前球的位置
            ball:SetPosition(px, py)
            -- if ball:GetNum() == 0 then
            --     print("BilliardsUpdate:----------------FixedUpdate+++++++++++++++:", px, py)
            -- end

            -- 角度不需要同步，不用fixed
            local distance = math.sqrt((px - posX) * (px - posX) + (py - posY) * (py - posY))
            local angle = (distance * 180 / math.pi) / BilliardsTable.BALL_RADIUS
            ball:ChangeAngle(angle)
        end

        :: continue ::
    end

    -- 检查球是否全部停止移动
    local leftBalls = BilliardsBattle.GetLeftBalls()
    for _, ball in ipairs(leftBalls) do
        if not ball:IsStopped() then
            return false
        end
    end

    BilliardsBattle.EndRound()
    return true
end

function BilliardsUpdate:Update()
    local balls = BilliardsBattle.GetLeftBalls()
    for i = 1, #balls do
        balls[i]:UpdateState()
    end

    ---转杆的中间状态
    if BilliardsBattle.CheckRoundPhase(BillardsType.RoundPhase.HIT_BALLS) then
        local dirs = BilliardsBattle.GetCueDirList()
        if dirs and #dirs > 0 then
            local px, py, vx, vy, snum, svx, svy = self:SimulateCast(dirs[1].x, dirs[1].y)
            local ball_x, ball_y = BilliardsBattle.GetWhiteBall():GetPosition()
            --- 被击的球
            local isForbid = false
            if snum then
                local ballComp = BilliardsBattle.GetBall(snum)
                local cur_player = BilliardsBattle.GetCurrentPlayer()
                --- 被击的是否是自己的球
                isForbid = cur_player:GetBallType() ~= 0 and cur_player:GetBallType() ~= ballComp:GetType()
            end
            BilliardsTable.DrawCue(true, isForbid, ball_x, ball_y, dirs[1].x, dirs[1].y, px, py, vx, vy, snum, svx, svy)
            table.remove(dirs, 1)
        else
            BilliardsBattle.SyncCueDir()
        end
    end

    ---自由球的中间状态
    if BilliardsBattle.CheckRoundPhase(BillardsType.RoundPhase.FREE_BALL) then
        local freePoints = BilliardsBattle.GetFreeBallPoints()
        if freePoints and #freePoints > 0 then
            BilliardsTable.UpdateFreeBallState(true, freePoints[1].x, freePoints[1].y)
            table.remove(freePoints, 1)
        else
            BilliardsBattle.SyncFreeBallPos()
        end
    end
end

function BilliardsUpdate:SimulatePositionAndVelocity(x, y, vx, vy, t)
    local friction = 0.98
    local tx = math.fixed3(vx * friction)
    local ty = math.fixed3(vy * friction)
    -- local tx = math.fixed3(vx * (1 - deltaTime * 1.1))
    -- local ty = math.fixed3(vy * (1 - deltaTime * 1.1))
    
    if math.abs(tx) < 3 then tx = 0 end
    if math.abs(ty) < 3 then ty = 0 end
    
    local cx = math.fixed3(x + tx * t)
    local cy = math.fixed3(y + ty * t)

    return cx, cy, tx, ty
end

function BilliardsUpdate:CollidesPocket(ball, px, py)
    for _, pocket in ipairs(BilliardsTable.pockets) do
        local bounds = pocket.bounds
        if px >= bounds.minX and px <= bounds.maxX and py >= bounds.minY and py <= bounds.maxY then
            -- 进入pocket的检测区域
            if self:IsTouched(px, py, pocket.hole.x, pocket.hole.y, pocket.hole.r, pocket.hole.r2) then
                BilliardsBattle.EnterPocket(ball, pocket.index)
                return true
            end
            break
        end
    end
    return false
end

function BilliardsUpdate:CollidesCircle(x1, y1, vx1, vy1, m1, x2, y2, vx2, vy2, m2, d1, d2)
    local touched, dist = self:IsTouched(x1, y1, x2, y2, d1, d2)
    if touched then
        -- self:ChangeVelocityAndDirection(cue, hit, deltaTime)
        vx1, vy1, vx2, vy2 = self:CalculateVelocity(x1, y1, vx1, vy1, m1, x2, y2, vx2, vy2, m2)
        
        -- 纠正坐标
        dist = math.fixed3(math.sqrt(dist))
        local od = math.fixed3(d1 - dist)
        local ox = math.fixed3(math.fixed3((x1 - x2) / dist) * od)
        local oy = math.fixed3(math.fixed3((y1 - y2) / dist) * od)
        return true, ox, oy, vx1, vy1, vx2, vy2
    end
    return false
end

function BilliardsUpdate:CollidesBall(cue, hit, px, py, vx, vy)
    if cue:IsStopped() and hit:IsStopped() then
        return false
    end
    local hx, hy = hit:GetPosition()
    local hvx, hvy = hit:GetVel()

    local touched, ox, oy, vx1, vy1, vx2, vy2 = self:CollidesCircle(px, py, vx, vy, 1, hx, hy, hvx, hvy, 1, BilliardsTable.BALL_DIAMETER, BilliardsTable.BALL_DIAMETER_SQUARE)
    if touched then
        cue:SetVel(vx1, vy1)
        hit:SetVel(vx2, vy2)
        BilliardsBattle.TrySetFirstBall(cue, hit)
    end

    return touched, ox, oy
    -- local touched, dist = self:IsTouched(px, py, hx, hy, BilliardsTable.BALL_DIAMETER)
    -- if touched then
    --     -- self:ChangeVelocityAndDirection(cue, hit, deltaTime)
    --     local vx1, vy1, vx2, vy2 = self:CalculateVelocity(px, py, vx, vy, 1, hx, hy, hvx, hvy, 1)
    --     cue:SetVel(vx1, vy1)
    --     hit:SetVel(vx2, vy2)
        
    --     dist = math.fixed3(math.sqrt(dist))
    --     local d = math.fixed3(BilliardsTable.BALL_DIAMETER - dist)
    --     return true, math.fixed3(math.fixed3((px - hx) / dist) * d), math.fixed3(math.fixed3((py - hy) / dist) * d)
    -- end
    -- return false
end

-- function BilliardsUpdate:CollidesPocketWall(ball, px, py, vx, vy)
--     -- 检测是否洞口圆角区域
--     for _, pocket in ipairs(BilliardsTable.pockets) do
--         local bounds = pocket.bounds
--         if px >= bounds.minX and px <= bounds.maxX and py >= bounds.minY and py <= bounds.maxY then
--             if self:IsTouched(px, py, pocket.fence1.x, pocket.fence1.y, pocket.fence1.r) then
--                 -- 碰撞1
--                 local tx, ty = self:CalculateVelocity(px, py, vx, vy, 1, pocket.fence1.x, pocket.fence1.y, 0, 0, 0)
--                 ball:SetVel(tx, ty)
--                 return true
--             elseif self:IsTouched(px, py, pocket.fence2.x, pocket.fence2.y, pocket.fence2.r) then
--                 -- 碰撞2
--                 local tx, ty = self:CalculateVelocity(px, py, vx, vy, 1, pocket.fence2.x, pocket.fence2.y, 0, 0, 0)
--                 ball:SetVel(tx, ty)
--                 return true
--             end
--             break
--         end
--     end
--     return false
-- end

-- function BilliardsUpdate:CollidesPocketFence(ball, fence, px, py, vx, vy)
--     local fx, fy = fence.x, fence.y
--     local touched, dist = self:IsTouched(px, py, fx, fy, fence.r)
--     if touched then
--         local tx, ty = self:CalculateVelocity(px, py, vx, vy, 1, fx, fy, 0, 0, 0)
--         ball:SetVel(tx, ty)

--         -- 
--         dist = math.fixed3(math.sqrt(dist))
--         local d = math.fixed3(fence.r - dist)
--         return true, math.fixed3(math.fixed3((px - fx) / dist) * d), math.fixed3(math.fixed3((py - fy) / dist) * d)
--     end

--     return false
-- end

function BilliardsUpdate:CollidesWall(ball, px, py, vx, vy)
    -- print("BilliardsUpdate:CollidesWall:=========1======", ball:GetNum(), px, py, vx, vy)
    -- 检测是否洞口圆角区域
    for _, pocket in ipairs(BilliardsTable.pockets) do
        local bounds = pocket.bounds
        if px >= bounds.minX and px <= bounds.maxX and py >= bounds.minY and py <= bounds.maxY then
            -- local hit, ox, oy = self:CollidesPocketFence(ball, pocket.fence1, px, py, vx, vy)
            -- if not hit then
            --     hit, ox, oy = self:CollidesPocketFence(ball, pocket.fence2, px, py, vx, vy)
            -- end
            local touched, ox, oy, vx1, vy1 = self:CollidesCircle(px, py, vx, vy, 1, pocket.fence1.x, pocket.fence1.y, 0, 0, 0, pocket.fence1.r, pocket.fence1.r2)
            if not touched then
                touched, ox, oy, vx1, vy1 = self:CollidesCircle(px, py, vx, vy, 1, pocket.fence2.x, pocket.fence2.y, 0, 0, 0, pocket.fence2.r, pocket.fence2.r2)
            end
            if touched then
                ball:SetVel(vx1, vy1)
            end
            return touched, ox, oy
        --     local touched, dist = self:IsTouched(px, py, pocket.fence1.x, pocket.fence1.y, pocket.fence1.r)
        --     if touched then

        --     end
            
        -- dist = math.fixed3(math.sqrt(dist))
        -- local d = math.fixed3(BilliardsTable.BALL_DIAMETER - dist)
        -- return true, math.fixed3(math.fixed3((px - hx) / dist) * d), math.fixed3(math.fixed3((py - hy) / dist) * d)
        --     if self:IsTouched(px, py, pocket.fence1.x, pocket.fence1.y, pocket.fence1.r) then
        --         -- 碰撞1
        --         local tx, ty = self:CalculateVelocity(px, py, vx, vy, 1, pocket.fence1.x, pocket.fence1.y, 0, 0, math.huge)
        --         print("CollidesWall:====1============", vx, vy, tx, ty,pocket.fence1.x, pocket.fence1.y, pocket.fence1.r)
        --         ball:SetVel(tx, ty)
        --         return true
        --     elseif self:IsTouched(px, py, pocket.fence2.x, pocket.fence2.y, pocket.fence2.r) then
        --         -- 碰撞2
        --         local tx, ty = self:CalculateVelocity(px, py, vx, vy, 1, pocket.fence2.x, pocket.fence2.y, 0, 0, math.huge)
        --         print("CollidesWall:=====2===========", vx, vy, tx, ty, pocket.fence2.x, pocket.fence2.y, pocket.fence2.r)
        --         ball:SetVel(tx, ty)
        --         return true
        --     end
        --     print("CollidesWall:=====0===========", px, py, vx, vy)
        --     return false
        end
    end

    -- 检测是否与台面矩形碰撞
    local hit = false
    local fence = BilliardsTable.fence
    local minX, minY, maxX, maxY = fence.minX, fence.minY, fence.maxX, fence.maxY
    -- print("fence:", minX, minY, maxX, maxY)
    if (py > maxY and vy > 0) or (py < minY and vy < 0) then  --撞上墙
        vy = -vy
        hit = true
    end
    if (px > maxX and vx > 0) or (px < minX and vx < 0) then
        vx = -vx
        hit = true
    end
    
    if hit then
        ball:SetVel(vx, vy)
        -- print("BilliardsUpdate:CollidesWall:========2=======", ball:GetNum(), px, py, vx, vy, hit, ball:GetVel())
    end

    return hit, 0, 0
end

function BilliardsUpdate:CalculateVelocity(x1, y1, vx1, vy1, m1, x2, y2, vx2, vy2, m2)
    local dx = x1 - x2
    local dy = y1 - y2

    local dist = math.fixed3(math.sqrt(dx * dx + dy * dy))
    -- print("BilliardsUpdate+++++++++++:CalculateVelocity:-----0-----", x1, y1, x2, y2, dx, dy, dist)

    -- normalize
    local nx1, ny1 = math.fixed3(dx / dist), math.fixed3(dy / dist)
    local nx2, ny2 = -ny1, nx1

    local v1n = math.fixed3(vx1 * nx1 + vy1 * ny1)
    local v1t = math.fixed3(vx1 * nx2 + vy1 * ny2)
    local v2n = math.fixed3(vx2 * nx1 + vy2 * ny1)
    local v2t = math.fixed3(vx2 * nx2 + vy2 * ny2)

    -- print("BilliardsUpdate+++++++++++:CalculateVelocity:-----1-----", dx, dy, nx1, ny1, nx2, ny2, v1n, v1t, v2n, v2t)

    -- 动量守恒
    if m1 == m2 then
        v1n, v2n = v2n, v1n
    else
        -- local v1nAfter = math.fixed3((v1n * (m1 - m2) + 2 * m2 * v2n) / (m1 + m2))
        -- local v2nAfter = math.fixed3((v2n * (m2 - m1) + 2 * m1 * v1n) / (m1 + m2))
        -- v1n, v2n = v1nAfter, v2nAfter

        v1n, v2n = -v1n, 0
    end
    
    vx1 = math.fixed3(nx1 * v1n + nx2 * v1t)
    vy1 = math.fixed3(ny1 * v1n + ny2 * v1t)
    vx2 = math.fixed3(nx1 * v2n + nx2 * v2t)
    vy2 = math.fixed3(ny1 * v2n + ny2 * v2t)
    -- print("BilliardsUpdate+++++++++++:CalculateVelocity:-----2-----", vx1, vy1, vx2, vy2, v1n, v1t, v2n, v2t)

    return vx1, vy1, vx2, vy2
end

-- function BilliardsUpdate:UpdateBallPosition(ball, deltaTime)
--     local friction = 0.99
--     local vx, vy = ball:GetVel()
--     local tx = math.fixed3(vx * friction)
--     local ty = math.fixed3(vy * friction)
--     -- local tx = math.fixed3(vx * (1 - deltaTime * 1.1))
--     -- local ty = math.fixed3(vy * (1 - deltaTime * 1.1))
    
--     if tx < 0.1 then tx = 0 end
--     if ty < 0.1 then ty = 0 end
    
--     local posX, posY = ball:GetPosition()
--     local cx = math.fixed3(posX + tx * deltaTime)
--     local cy = math.fixed3(posY + ty * deltaTime)
--     ball:SetData(cx, cy, tx, ty)
--     print("BilliardsUpdate+++++:UpdateBallPosition:-----------", ball:GetNum(), deltaTime, vx, vy, tx, ty, cx, cy, posX, posY)

--     -- 角度不需要同步，不用fixed
--     local distance = math.sqrt((cx - posX) * (cx - posX) + (cy - posY) * (cy - posY))
--     local angle = (distance * 180 / math.pi) / BilliardsTable.BALL_RADIUS
--     ball:ChangeAngle(angle)
-- end

-- function BilliardsUpdate:CollideBalls(ball, deltaTime)
--     -- print("BilliardsUpdate+++++++++:CollideBalls:-------", self:CalculateVelocity(0, 111.3937, 0, 452.8315, 1, -0.035, 126.4308, 0, 0, 1))
--     local balls = BilliardsBattle.GetLeftBalls()
--     for _, target in ipairs(balls) do
--         if ball ~= target then
--             local cx1, cy1 = ball:GetPosition()
--             local cx2, cy2 = target:GetPosition()
--             if self:IsTouched(cx1, cy1, cx2, cy2, BilliardsTable.BALL_DIAMETER) then
--                 self:ChangeVelocityAndDirection(ball, target, deltaTime)
--                 -- self:ChangeSpeed(ball, target, deltaTime)
--                 -- BilliardsAudio.PlaySound("Ball_hit_01")
--                 -- BilliardsBattle.TrySetFirstBall(ball, target)
--             end
--         end
--     end
-- end

-- function BilliardsUpdate:ChangeVelocityAndDirection(cueBall, dstBall, deltaTime)
--     local x1, y1 = cueBall:GetPosition()
--     local x2, y2 = dstBall:GetPosition()
--     local vx1, vy1 = cueBall:GetVel()
--     local vx2, vy2 = dstBall:GetVel()
--     -- print("BilliardsUpdate+++++:ChangeVelocityAndDirection:--------------", cueBall:GetNum(), dstBall:GetNum(), x1, y1, x2, y2, vx1, vy1, vx2, vy2)

--     -- 计算反弹速度
--     vx1, vy1, vx2, vy2 = self:CalculateVelocity(x1, y1, vx1, vy1, 1, x2, y2, vx2, vy2, 1)
--     print("BilliardsUpdate+++++:ChangeVelocityAndDirection 1:--------------", cueBall:GetNum(), dstBall:GetNum(), vx1, vy1, vx2, vy2)
--     cueBall:SetVel(vx1, vy1)
--     dstBall:SetVel(vx2, vy2)

--     -- print(self:CalculateVelocity(0, 111.3937, 0, 452.8315, 1, -0.035, 126.4308, 0, 0, 1))

--     -- let velocity1 = new Vector(this.vx, this.vy);
--     -- let velocity2 = new Vector(other.vx, other.vy);
--     -- let vNorm = new Vector(this.x - other.x, this.y - other.y);
--     -- let unitVNorm = vNorm.normalize();
--     -- let unitVTan = new Vector(-unitVNorm.y, unitVNorm.x);
--     -- let v1n = velocity1.dot(unitVNorm);
--     -- let v1t = velocity1.dot(unitVTan);

--     -- let v2n = velocity2.dot(unitVNorm);
--     -- let v2t = velocity2.dot(unitVTan);
--     -- let v1nAfter = (v1n * (this.mass - other.mass) + 2 * other.mass * v2n) / (this.mass + other.mass);
--     -- let v2nAfter = (v2n * (other.mass - this.mass) + 2 * this.mass * v1n) / (this.mass + other.mass);
--     -- //简化----------------------------------------
--     -- let v1nAfter = v2n;
--     -- let v2nAfter = v1n;
--     -- let v1VectorNorm = unitVNorm.multiply(v1nAfter);
--     -- let v1VectorTan = unitVTan.multiply(v1t);

--     -- let v2VectorNorm = unitVNorm.multiply(v2nAfter);
--     -- let v2VectorTan = unitVTan.multiply(v2t);
--     -- let velocity1After = v1VectorNorm.add(v1VectorTan);
--     -- let velocity2After = v2VectorNorm.add(v2VectorTan);
--     -- this.vx = velocity1After.x;
--     -- this.vy = velocity1After.y;
--     -- other.vx = velocity2After.x;
--     -- other.vy = velocity2After.y;
-- end

-- 0.0	111.3937	-0.035	126.4308	0	452.8315	0	0

-- function BilliardsUpdate:ChangeSpeed(ball, target, deltaTime)
--     local x1, y1 = ball:GetPosition()
--     local x2, y2 = target:GetPosition()
--     local vx1, vy1 = ball:GetVel()
--     local vx2, vy2 = target:GetVel()

--     vx1, vy1, vx2, vy2 = self:CollideCircle(x1, y1, vx1, vy1, x2, y2, vx2, vy2)
--     ball:SetVel(vx1, vy1)
--     target:SetVel(vx2, vy2)

--     local tx1 = x1 + math.fixed3(deltaTime * vx1)
--     local ty1 = y1 + math.fixed3(deltaTime * vy1)
    
--     local tx2 = x2 + math.fixed3(deltaTime * vx2)
--     local ty2 = y2 + math.fixed3(deltaTime * vy2)

--     if self:IsTouched(tx1, ty1, tx2, ty2, BilliardsTable.BALL_DIAMETER) then
--         local dx = tx1 - tx2
--         local dy = ty1 - ty2
--         local dist = math.fixed3(math.sqrt(dx * dx + dy * dy))
--         local p1 = math.fixed3(dx / dist)
--         local p2 = math.fixed3(dy / dist)
--         local p3 = math.fixed3(-dx / dist)
--         local p4 = math.fixed3(-dy / dist)
--         ball:SetPosition(x1 + math.fixed3(deltaTime * p1), y1 + math.fixed3(deltaTime * p2))
--         target:SetPosition(x2 + math.fixed3(deltaTime * p3), y2 + math.fixed3(deltaTime * p4))
--     end
-- end

---是否接触
function BilliardsUpdate:IsTouched(x1, y1, x2, y2, d1, d2)
    local dx = x1 - x2
    local dy = y1 - y2
    if dx < d1 and dy < d1 then
        local dist = math.fixed3(dx * dx + dy * dy)
        return dist < d2, dist
    end
    return false
end

-- function BilliardsUpdate:CollideCircle(posX, posY, vx, vy, targetX, targetY, targetVx, targetVy)
--     -- m1 * Vx + m2 * Ux = m1 * Vx' + m2 * Ux'
--     local dx = targetX - posX
--     local dy = targetY - posY
--     local dist = math.fixed3(math.sqrt(dx * dx + dy * dy))
--     local px = math.fixed3(dx / dist)  --这边是单位化
--     local py = math.fixed3(dy / dist)

--     local ux1 = math.fixed3(math.fixed3(px * vx + py * vy) * px)
--     local uy1 = math.fixed3(math.fixed3(px * vx + py * vy) * py)
--     local ux2 = vx - ux1
--     local uy2 = vy - uy1

--     local ux3 = math.fixed3(math.fixed3(px * targetVx + py * targetVy) * px)
--     local uy3 = math.fixed3(math.fixed3(py * targetVx + py * targetVy) * py)
--     local ux4 = targetVx - ux3
--     local uy4 = targetVy - uy3

--     vx = ux2 + ux3
--     vy = uy2 + uy3
--     targetVx = ux1 + ux4
--     targetVy = uy1 + uy4
--     return vx, vy, targetVx, targetVy
-- end

-- function BilliardsUpdate:CollideTable(ball, deltaTime)
--     local vx, vy = ball:GetVel()
--     local posX, posY = ball:GetPosition()

--     for _, pocket in ipairs(BilliardsTable.pockets) do
--         local bounds = pocket.bounds
--         if posX >= bounds.minX and posX <= bounds.maxX and posY >= bounds.minY and posY <= bounds.maxY then
--             -- 进入pocket的检测区域
--             if self:IsTouched(posX, posY, pocket.hole.x, pocket.hole.y, pocket.hole.r) then
--                 -- 落袋
--                 BilliardsAudio.PlaySound("Ball_in_01")
--                 BilliardsBattle.EnterPocket(ball, pocket.index)
--             else
--                 if self:IsTouched(posX, posY, pocket.fence1.x, pocket.fence1.y, pocket.fence1.r) then
--                     -- 碰撞1
--                     vx, vy = self:CollideCircle(posX, posY, vx, vy, pocket.fence1.x, pocket.fence1.y, vx, vy)
--                     ball:SetVel(vx, vy)
--                 elseif self:IsTouched(posX, posY, pocket.fence2.x, pocket.fence2.y, pocket.fence2.r) then
--                     -- 碰撞2
--                     vx, vy = self:CollideCircle(posX, posY, vx, vy, pocket.fence2.x, pocket.fence2.y, vx, vy)
--                     ball:SetVel(vx, vy)
--                 end
--             end
--             return
--         end
--     end

--     -- 检测是否与台面矩形碰撞
--     local hit = false
--     local fence = BilliardsTable.fence
--     local minX, minY, maxX, maxY = fence.minX, fence.minY, fence.maxX, fence.maxY
--     -- print("fence:", minX, minY, maxX, maxY)
--     if (posY > maxY and vy > 0) or (posY < minY and vy < 0) then  --撞上墙
--         vy = -vy
--         hit = true
--     end
--     if (posX > maxX and vx > 0) or (posX < minX and vx < 0) then
--         vx = -vx
--         hit = true
--     end

--     if hit then
--         ball:SetVel(vx, vy)
--         BilliardsAudio.PlaySound("Ball_rail_01")
--     end
-- end

function BilliardsUpdate:SimulateCast(dirX, dirY)
    local ball = BilliardsBattle.GetWhiteBall()
    local balls = BilliardsBattle.GetLeftBalls()

    local pockets = BilliardsTable.pockets
    local diameter = BilliardsTable.BALL_DIAMETER
    local diameter2 = BilliardsTable.BALL_DIAMETER_SQUARE

    local fence = BilliardsTable.fence
    local minX, minY, maxX, maxY = fence.minX, fence.minY, fence.maxX, fence.maxY

    local deltaTime = math.fixed3(0.02)
    local px, py = ball:GetPosition()
    local vx, vy = math.fixed3(dirX * 1500), math.fixed3(dirY * 1500)
    -- 碰撞球的速度（方向）
    local snum, svx, svy = nil

    -- 方向为0，不需要计算
    if dirX == 0 and dirY == 0 then
        return px, py, 0, 0
    end

    -- local sb = {"BilliardsUpdate:----------SimulateCast:++++begin+++++++\n"}
    -- -- print("BilliardsUpdate:----------SimulateCast:++++begin+++++++")

    local count = 1
    while count <= 100 do
        count = count + 1

        local posX, posY, tx, ty = px, py, vx, vy
        -- 拆分时间片
        local slices = math.max(math.ceil(math.fixed3(math.max(math.abs(tx), math.abs(ty)) * deltaTime)), 1)
        -- print("SimulateCast+++++++++++++++++", count, posX, posY, tx, ty, slices)
        for step = 1, slices do
            px, py, vx, vy = self:SimulatePositionAndVelocity(posX, posY, tx, ty, math.fixed3((deltaTime * step) / slices))
            -- sb[#sb + 1] = string.format("BilliardsUpdate:----------SimulateCast:+++++++++++, %s, %s, %s, %s, %s, %s, %s, %s, %s, %s, %s \n", px, py, vx, vy, posX, posY, tx, ty, step, slices, math.fixed3((deltaTime * step) / slices))
            -- print("BilliardsUpdate:----------SimulateCast:+++++++++++", px, py, vx, vy, posX, posY, tx, ty, step, slices, math.fixed3((deltaTime * step) / slices))
            -- 检查是否进袋
            for _, pocket in ipairs(pockets) do
                local bounds = pocket.bounds
                if px >= bounds.minX and px <= bounds.maxX and py >= bounds.minY and py <= bounds.maxY then
                    -- 进入pocket的检测区域
                    if self:IsTouched(px, py, pocket.hole.x, pocket.hole.y, pocket.hole.r, pocket.hole.r2) then
                        -- 往洞口里多显示半个球
                        local dx, dy = BillardsUtil.Vector2Normalize(vx, vy)
                        px = px + math.fixed3(dx * BilliardsTable.BALL_RADIUS)
                        py = py + math.fixed3(dy * BilliardsTable.BALL_RADIUS)
                        vx, vy = 0, 0
                        goto result
                    end
                    break
                end
            end

            -- 检查是否与球碰撞
            for j = 1, #balls do
                local hit = balls[j]
                if ball ~= hit then
                    local hx, hy = hit:GetPosition()
                    local hvx, hvy = hit:GetVel()
                    
                    local touched, ox, oy, vx1, vy1, vx2, vy2 = self:CollidesCircle(px, py, vx, vy, 1, hx, hy, hvx, hvy, 1, diameter, diameter2)
                    if touched then
                        snum = hit:GetNum()
                        svx, svy = vx2, vy2
                        vx, vy = vx1, vy1
                        px, py = px + ox, py + oy
                        -- sb[#sb + 1] = string.format("BilliardsUpdate:----------SimulateCast:+++++touched++++++, %s, %s, %s, %s, %s \n", touched, px, py, vx, vy)
                        -- print(table.concat(sb))
                        -- print("BilliardsUpdate:----------SimulateCast:+++++touched++++++", touched, px, py, vx, vy)
                        goto result
                    end
                end
            end

            -- 检查是否与桌子碰撞
            for _, pocket in ipairs(pockets) do
                local bounds = pocket.bounds
                if px >= bounds.minX and px <= bounds.maxX and py >= bounds.minY and py <= bounds.maxY then
                    local touched, ox, oy, vx1, vy1 = self:CollidesCircle(px, py, vx, vy, 1, pocket.fence1.x, pocket.fence1.y, 0, 0, 0, pocket.fence1.r, pocket.fence1.r2)
                    if not touched then
                        touched, ox, oy, vx1, vy1 = self:CollidesCircle(px, py, vx, vy, 1, pocket.fence2.x, pocket.fence2.y, 0, 0, 0, pocket.fence2.r, pocket.fence2.r2)
                    end
                    if touched then
                        vx, vy = vx1, vy1
                        px, py = px + ox, py + oy
                        goto result
                    end
                    goto continue
                end
            end
            
            -- 检测是否与台面矩形碰撞
            local touched = false
            if (py > maxY and vy > 0) or (py < minY and vy < 0) then  --撞上墙
                vy = -vy
                touched = true
            end
            if (px > maxX and vx > 0) or (px < minX and vx < 0) then
                vx = -vx
                touched = true
            end
            if touched then
                goto result
            end

            :: continue ::
        end
    end

    :: result ::

    -- print("SimulateCast------------------------", count, px, py, vx, vy, svx, svy)
    local magnitude = math.fixed3(math.sqrt(vx * vx + vy * vy))
    if magnitude > 1e-05 then
        vx, vy = math.fixed3(vx / magnitude), math.fixed3(vy / magnitude)
    else
        vx, vy = 0, 0
    end
    if svx and svy then
        magnitude = math.fixed3(math.sqrt(svx * svx + svy * svy))
        if magnitude > 1e-05 then
            svx, svy = math.fixed3(svx / magnitude), math.fixed3(svy / magnitude)
        else
            svx, svy = 0, 0
        end
    end

    return px, py, vx, vy, snum, svx, svy
end

function BilliardsUpdate:SimulateMove(time)
    local steps = math.floor((time / 1000) / 0.02)
    print("BilliardsBattle:BilliardsUpdate:SimulateMove:+++++", time, steps)
    for step = 1, steps do
        if self:UpdateMove(true) then
            break
        end
    end
end

return BilliardsUpdate