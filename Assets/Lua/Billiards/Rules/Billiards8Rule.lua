local BilliardsBallType = BillardsType.BillardsBallType

local Billiards8Rule = class("Billiards8Rule", BilliardsRule)

-- 白球和黑球的初始位置
Billiards8Rule.whiteBallPos = {x = 0, y = -180}
Billiards8Rule.blackBallPos = {x = 0, y = 185}

Billiards8Rule._points = nil

function Billiards8Rule:initialize()
    -- 固定摆球布局
    local deltaX = BilliardsTable.BALL_RADIUS
    local deltaY = math.fixed3(math.sqrt(deltaX * deltaX * 3))
    local initPosX, initPosY = self.blackBallPos.x, self.blackBallPos.y

    self._points = {
        -- 白球
        { num = 0,  x = self.whiteBallPos.x,   y = self.whiteBallPos.y   },

        { num = 1,  x = initPosX,              y = initPosY - deltaY * 2 },

        { num = 13, x = initPosX - deltaX,     y = initPosY - deltaY     },
        { num = 3,  x = initPosX + deltaX,     y = initPosY - deltaY     },

        { num = 12, x = initPosX - deltaX * 2, y = initPosY              },
        { num = 8,  x = initPosX,              y = initPosY              },
        { num = 14, x = initPosX + deltaX * 2, y = initPosY              },

        { num = 6,  x = initPosX - deltaX * 3, y = initPosY + deltaY     },
        { num = 11, x = initPosX - deltaX,     y = initPosY + deltaY     },
        { num = 9,  x = initPosX + deltaX,     y = initPosY + deltaY     },
        { num = 10, x = initPosX + deltaX * 3, y = initPosY + deltaY     },

        { num = 2,  x = initPosX - deltaX * 4, y = initPosY + deltaY * 2 },
        { num = 5,  x = initPosX - deltaX * 2, y = initPosY + deltaY * 2 },
        { num = 7,  x = initPosX,              y = initPosY + deltaY * 2 },
        { num = 4,  x = initPosX + deltaX * 2, y = initPosY + deltaY * 2 },
        { num = 15, x = initPosX + deltaX * 4, y = initPosY + deltaY * 2 },
    }
end

function Billiards8Rule:GetInitPoints()
    return self._points
end

function Billiards8Rule:GetBallType(num)
    if num >= 1 and num <= 7 then
        -- 单球
        return BilliardsBallType.SINGLE_BALL
    end

    if num == 8 then
        -- 黑球
        return BilliardsBallType.BLACK_BALL
    end

    if num >= 9 and num <= 15 then
        -- 彩球
        return BilliardsBallType.COLOR_BALL
    end

    -- 白球
    return BilliardsBallType.WHITE_BALL
end

function Billiards8Rule:BeginRound()
end

function Billiards8Rule:EndRound()
    local points = {}

    local leftBalls = BilliardsBattle.GetLeftBalls()
    for i = 1, #leftBalls do
        local ball = leftBalls[i]
        local posX, posY = ball:GetPosition()
        table.insert(points, {num = ball.num, x = posX, y = posY})
    end
    
    local inpockets = BilliardsBattle.ballsInPockets

    for i = 1, #inpockets do
        local ballNum = inpockets[i]
        if ballNum == 8 then
            local ballPos = self:GetBlackBallPos()
            table.insert(points, {num = 8, x = ballPos.x, y = ballPos.y})
        elseif ballNum == 0 then
            local whiteBallPos = self:GetWhiteBallPos()
            table.insert(points, {num = 0, x = whiteBallPos.x, y = whiteBallPos.y})
        end
    end

    BilliardsProxy.Net2S_5104(BilliardsBattle.firstHitBall, inpockets, points)
end

function Billiards8Rule:GetWhiteBallPos(init)
    if init then
        return self.whiteBallPos
    end
    return self:GetValidPos(self.whiteBallPos)
end

function Billiards8Rule:GetBlackBallPos(init)
    if init then
        return self.blackBallPos
    end
    return self:GetValidPos(self.blackBallPos)
end

function Billiards8Rule:CanHit(ballNum, hitType, canHitBalls, pocketBalls)
    return false
end

-- 判断落袋球是否需要重置
function Billiards8Rule:ShouldRespot(ballNum, hitType, canHitBalls, pocketBalls)
    if ballNum == 0 then return true end
    
    if ballNum == 8 then
        if #canHitBalls == 0 then
            -- 简单判断已落袋的球数小于7，进袋无效
            return #pocketBalls < 7
        else
            return canHitBalls[1] ~= 8
        end
    end

    return false
end

function Billiards8Rule:IsValidShot(ballNum, hitType, canHitBalls, pocketBalls)
    local result = self:ShouldRespot(ballNum, hitType, canHitBalls, pocketBalls)
    if result then return false end

    if hitType ~= 0 then
        return hitType == self:GetBallType(ballNum)
    end

    return true
end

function Billiards8Rule:GetValidPos(pos)
    local posX, posY = pos.x, pos.y
    local find = true
    local searchDistance = BilliardsTable.BALL_DIAMETER
    local ballList = BilliardsBattle.GetLeftBalls()

    local search_L = false
    local search_T = false

    local right_notfind = false
    local left_notfind = false
    local up_notfind = false
    local bottom_notfind = false

    local search_HIndex = 0
    local search_VIndex = 0
    local minX, minY, maxX, maxY = BilliardsTable.GetBounds()

    local findFunc = function(x, y)
        for i = 1, #ballList do
            local ball = ballList[i]
            local pos_x, pos_y = ball:GetPosition()
            local distance = math.fixed3(math.sqrt((x - pos_x) * (x - pos_x) + (y - pos_y) * (y - pos_y)))
            if distance <= BilliardsTable.BALL_DIAMETER then
                return false
            end
        end
        return true
    end

    find = findFunc(posX, posY)

    while(not find) do

        local index = math.ceil(search_HIndex / 2)
        if (not search_L) and (not left_notfind) then
            posX = pos.x - index * searchDistance
            search_L = true
        else
            posX = pos.x + index * searchDistance
            search_L = false
        end
        
        if posX <= minX then
            left_notfind = true
        elseif posX >= maxX then
            right_notfind = true
        end
        
        if left_notfind and right_notfind and up_notfind and bottom_notfind then
            find = true
            posX = pos.x
            posY = pos.y
            break
        else
            up_notfind = false
            bottom_notfind = false
        end

        search_HIndex = search_HIndex + 1
        search_VIndex = 0

        while(not find) do
            local index = math.ceil(search_VIndex / 2)
            if (not search_T) and (not up_notfind) then
                posY = pos.y + index * searchDistance
                search_T = true
            else
                posY = pos.y - index * searchDistance
                search_T = false
            end

            search_VIndex = search_VIndex + 1

            if posY <= minY then
                bottom_notfind = true
                find = false
            elseif posY >= maxY then
                up_notfind = true
                find = false
            end

            if bottom_notfind and up_notfind then
                break
            else
                if posY > minY and posY < maxY then
                    find = findFunc(posX, posY)
                    if find then
                        break
                    end
                end
            end
        end
    end

    return {x = posX, y = posY}
end

-- function Billiards8Rule.ResetBlack8Pos(pos)
--     local ballComp = self.balls[8]
--     ballComp:SetVel(0, 0)
--     ballComp:SetPosition(pos)
--     ballComp.gameObject:SetActive(true)
--     BillardsInfo.addBall(8, ballComp)
-- end

-- function Billiards8Rule.ResetWhiteBallPos(pos)
--     local ballComp = self.balls[0]
--     ballComp:SetVel(0, 0)
--     ballComp:SetPosition(pos)
--     BillardsInfo.addBall(0, ballComp)
--     ballComp.gameObject:SetActive(true)
-- end

return Billiards8Rule