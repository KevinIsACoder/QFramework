local BilliardsRule = class("BilliardsRule")

function BilliardsRule:GetInitPoints()
    return {}
end

function BilliardsRule:BeginRound()
end

function BilliardsRule:EndRound()
end

function BilliardsRule:GetWhiteBall(init)
    return {}
end

function BilliardsRule:GetBallType(num)
    return 0
end

function BilliardsRule:CanHit(ballNum, hitType, canHitBalls, pocketBalls)
    return false
end
function BilliardsRule:ShouldRespot(ballNum, hitType, canHitBalls, pocketBalls)
    return false
end
function BilliardsRule:IsValidShot(ballNum, hitType, canHitBalls, pocketBalls)
    return false
end

return BilliardsRule